using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using ZWave4Net.Channel.Protocol.Frames;
using ZWave4Net.Diagnostics;
using ZWave4Net.Utilities;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ZWave4Net.Channel.Protocol
{
    public class MessageBroker
    {
        private readonly ILogger _logger = Logging.Factory.CreatLogger("Broker");
        private readonly FrameReader _reader;
        private readonly FrameWriter _writer;

        private IConnectableObservable<Frame> _observable;

        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private Task _task;

        public MessageBroker(IDuplexStream stream)
        {
            _reader = new FrameReader(stream);
            _writer = new FrameWriter(stream);
        }

        public void Run(CancellationToken cancellation)
        {
            // create the Observable, use Publish so frame are all published to all subcribers 
            _observable = Observable.Create<Frame>(observer => Execute(observer, cancellation)).Publish();

            // connect the observerable (start running)
            var subscription = _observable.Connect();

            // when canceled dispose subscription
            cancellation.Register(() =>
            {
                subscription.Dispose();
            });
        }

        private Task Execute(IObserver<Frame> observer, CancellationToken cancellation)
        {

            return _task = Task.Run(async () =>
            {
                _logger.LogError("Starting Task");

                // execute until externally cancelled
                while (!cancellation.IsCancellationRequested)
                {
                    var frame = default(Frame);
                    try
                    {
                        // read next frame
                        frame = await _reader.Read(cancellation);
                    }
                    catch (StreamClosedException ex)
                    {
                        // most likely removal of ZStick from host
                        _logger.LogDebug(ex.Message);

                        observer.OnError(ex);

                        return;
                    }
                    catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                    {
                        observer.OnCompleted();

                        // the read was cancelled by the passed cancellationtoken so end gracefully 
                        break;
                    }
                    catch (ChecksumException ex)
                    {
                        // checksum failure on received frame, might happen during startup when we 
                        // are not synchronized and receive a partially frame
                        _logger.LogWarning(ex.Message);

                        // send NACK and hopefully the controller will send this frame again
                        _logger.LogDebug($"Writing: {Frame.NAK}");
                        await _writer.Write(Frame.NAK, cancellation);

                        // wait for next frame
                        continue;
                    }

                    // 
                    if (frame == Frame.ACK || frame == Frame.NAK || frame == Frame.CAN)
                    {
                        if (frame == Frame.ACK)
                            _logger.LogDebug($"Received: {frame}");
                        else
                            _logger.LogWarning($"Received: {frame}");

                        // publish the frame
                        observer.OnNext(frame);

                        // wait for next frame
                        continue;
                    }

                    if (frame is DataFrame dataFrame)
                    {
                        _logger.LogDebug($"Received: {frame}");

                        // dataframes must be aknowledged
                        _logger.LogDebug($"Writing: {Frame.ACK}");
                        await _writer.Write(Frame.ACK, cancellation);

                        // publish the frame
                        observer.OnNext(dataFrame);

                        // wait for next frame
                        continue;
                    }
                }

                _logger.LogError("Task Completed");

            }, cancellation);
        }

        public static Message Decode(DataFrame frame)
        {
            switch (frame.Type)
            {
                // response on a request
                case DataFrameType.RES:
                    // so create ResponseMessage
                    return new ResponseMessage(frame.Payload);

                // unsolicited event
                case DataFrameType.REQ:
                    // so create EventMessage
                    return new EventMessage(frame.Payload);
            }

            throw new ProtocolException("Invalid DataFrame type");
        }

        public TaskAwaiter GetAwaiter()
        {
            return _task?.GetAwaiter() ?? default(TaskAwaiter);
        }

        public static DataFrame Encode(RequestMessage message)
        {
            return new DataFrame(DataFrameType.REQ, message.Payload);
        }

        public IObservable<Message> GetObservable()
        {
            return _observable.OfType<DataFrame>().Select(element => Decode(element));
        }

        public async Task Send(RequestMessage message, CancellationToken cancellation)
        {
            var stopwatch = Stopwatch.StartNew();

            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.5.2 Request/Response frame flow
            // Note that due to the simple nature of the simple acknowledge mechanism, only one REQ->RES session is allowed.
            await _sendLock.WaitAsync(cancellation);
            try
            {
                // number of retransmissions
                var retransmissions = 0;

                // return only on ACK or Exception
                while (true)
                {
                    var completion = new TaskCompletionSource<Frame>(TaskCreationOptions.RunContinuationsAsynchronously);

                    // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 5.1 ACK frame
                    // The host MUST wait for a period of 1500ms before timing out waiting for the ACK frame
                    // timeoutCancellation.CancelAfter(ProtocolSettings.ACKWaitTime);
                    var chain = _observable
                        .Where(frame => frame == Frame.ACK || frame == Frame.NAK || frame == Frame.CAN)
                        .Timeout(ProtocolSettings.ACKWaitTime);

                    // set completion cancelled when token gets cancelled
                    using (cancellation.Register(() => completion.TrySetCanceled()))
                    {
                        // start listening for received frames, call onVerifyResponse for every received frame 
                        using (var subscription = chain.Subscribe
                        (
                            (element) => completion.TrySetResult(element),
                            (error) => completion.TrySetException(error))
                        )
                        {
                            // encode the message to a dataframe
                            var frame = Encode(message);

                            if (retransmissions == 0)
                                _logger.LogDebug($"Sending: {frame}");
                            else
                                _logger.LogWarning($"Resending: {frame}, attempt: {retransmissions}");

                            // send the request
                            await _writer.Write(frame, cancellation);

                            // mesasure time until frame received
                            stopwatch.Restart();

                            try
                            {
                                // wait for validated response
                                var response = await completion.Task;

                                // ACK received, so where done 
                                if (response == Frame.ACK)
                                    break;

                                // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.3 Retransmission
                                // A host or Z-Wave chip MUST NOT carry out more than 3 retransmissions
                                if (retransmissions >= ProtocolSettings.MaxRetryAttempts)
                                {
                                    if (response == Frame.CAN)
                                        throw new CanResponseException();
                                    if (response == Frame.NAK)
                                        throw new NakResponseException();
                                }
                            }
                            catch (TaskCanceledException) when (cancellation.IsCancellationRequested)
                            {
                                // operation was externally canceled, so rethrow
                                throw;
                            }
                            catch (TimeoutException)
                            {
                                // operation timed-out
                                _logger.LogWarning($"Timeout while waiting for an ACK");

                                // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.3 Retransmission
                                // A host or Z-Wave chip MUST NOT carry out more than 3 retransmissions
                                if (retransmissions >= ProtocolSettings.MaxRetryAttempts)
                                    throw new TimeoutException("Timeout while waiting for an ACK");
                            }
                        }
                    }
                    // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.3 Retransmission
                    // Twaiting = 100ms + n*1000ms 
                    // where n is incremented at each retransmission. n = 0 is used for the first waiting period.
                    var waitTime = ProtocolSettings.RetryDelayWaitTime.TotalMilliseconds + (retransmissions++ * ProtocolSettings.RetryAttemptWaitTime.TotalMilliseconds);

                    // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.2.2 Data frame delivery timeout
                    // The transmitter MAY compensate for the 1600ms already elapsed when calculating the retransmission waiting period
                    waitTime -= stopwatch.ElapsedMilliseconds;
                    if (waitTime > 0)
                    {
                        await Task.Delay((int)waitTime, cancellation);
                    }
                }
            }
            finally
            {
                // done, so release lock, other send operations allowed
                _sendLock.Release();
            }
        }
    }
}
