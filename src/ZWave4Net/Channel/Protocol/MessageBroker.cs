using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using ZWave.Channel.Protocol.Frames;
using ZWave.Diagnostics;
using ZWave.Utilities;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ZWave.Channel.Protocol
{
    internal class MessageBroker
    {
        private readonly ILogger _logger = Logging.Factory.CreatLogger("Broker");
        private readonly FrameReader _reader;
        private readonly FrameWriter _writer;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        private IConnectableObservable<Frame> _observable;

        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private Task _task;

        public MessageBroker(IDuplexStream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            _reader = new FrameReader(stream);
            _writer = new FrameWriter(stream);
        }

        public void Run(CancellationToken cancellationToken = default(CancellationToken))
        {
            // create the Observable, use Publish so frame are all published to all subcribers 
            _observable = Observable.Create<Frame>(observer => Execute(observer, cancellationToken)).Publish();

            // connect the observerable (start running)
            var subscription = _observable.Connect();

            // when canceled dispose subscription
            cancellationToken.Register(() =>
            {
                subscription.Dispose();
            });
        }

        private Task Execute(IObserver<Frame> observer, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            return _task = Task.Run(async () =>
            {
                _logger.LogDebug("Started");

                // execute until externally cancelled
                while (!cancellationToken.IsCancellationRequested)
                {
                    var frame = default(Frame);
                    try
                    {
                        // read next frame
                        frame = await _reader.Read(cancellationToken);
                    }
                    catch (StreamClosedException ex)
                    {
                        // most likely removal of ZStick from host
                        _logger.LogDebug(ex.Message);

                        observer.OnError(ex);

                        return;
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
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
                        await _writer.Write(Frame.NAK, cancellationToken);

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
                        await _writer.Write(Frame.ACK, cancellationToken);

                        // publish the frame
                        observer.OnNext(dataFrame);

                        // wait for next frame
                        continue;
                    }
                }

                _logger.LogDebug("Completed");

            }, cancellationToken);
        }

        public static Message Decode(DataFrame frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

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
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return new DataFrame(DataFrameType.REQ, message.Payload);
        }

        public IObservable<Message> Messages
        {
            get { return _observable.OfType<DataFrame>().Select(element => Decode(element)); }
        }

        public async Task Send(RequestMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.5.2 Request/Response frame flow
            // Note that due to the simple nature of the simple acknowledge mechanism, only one REQ->RES session is allowed.
            await _sendLock.WaitAsync(cancellationToken);
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
                    using (cancellationToken.Register(() => completion.TrySetCanceled()))
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

                            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.3 Retransmission
                            // Twaiting = 100ms + n*1000ms 
                            // where n is incremented at each retransmission. n = 0 is used for the first waiting period.
                            var waitTime = ProtocolSettings.RetryDelayWaitTime.TotalMilliseconds + (retransmissions++ * ProtocolSettings.RetryAttemptWaitTime.TotalMilliseconds);

                            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.2.2 Data frame delivery timeout
                            // The transmitter MAY compensate for the 1600ms already elapsed when calculating the retransmission waiting period
                            waitTime -= _stopwatch.ElapsedMilliseconds;
                            if (waitTime > 0)
                            {
                                await Task.Delay((int)waitTime, cancellationToken);
                            }

                            // send the request
                            await _writer.Write(frame, cancellationToken);

                            // restart timing
                            _stopwatch.Restart();

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
                            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
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
