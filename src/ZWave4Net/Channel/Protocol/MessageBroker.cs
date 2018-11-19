using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Utilities;
using System.Runtime.CompilerServices;
using ZWave4Net.Channel.Protocol.Frames;

namespace ZWave4Net.Channel.Protocol
{
    public class MessageBroker
    {
        private readonly ILogger _logger = Logging.CreatLogger("MessageBroker");
        private readonly FrameReader _reader;
        private readonly FrameWriter _writer;
        private readonly Publisher _publisher = new Publisher();
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private Task _task;

        public MessageBroker(IDuplexStream stream)
        {
            _reader = new FrameReader(stream);
            _writer = new FrameWriter(stream);
        }

        public TaskAwaiter GetAwaiter()
        {
            return _task?.GetAwaiter() ?? default(TaskAwaiter);
        }

        public void Run(CancellationToken cancellation)
        {
            if (_task != null)
                throw new InvalidOperationException("Broker already running");

            _task = Task.Run(async () =>
            {
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
                        throw; 
                    }
                    catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                    {
                        // the read was cancelled by the passed cancellationtoken so end gracefully 
                        break;
                    }
                    catch (ChecksumException ex)
                    {
                        // checksum failure on received frame, might happen during startup when we 
                        // are not synchronized and receive a partially frame
                        _logger.LogWarning(ex.Message);

                        // send NACK and hopefully the controller will send this frame again
                        _logger.LogDebug($"Writing {Frame.NAK}");
                        await _writer.Write(Frame.NAK, cancellation);

                        // wait for next frame
                        continue;
                    }

                    // 
                    if (frame == Frame.ACK || frame == Frame.NAK || frame == Frame.CAN)
                    {
                        if (frame == Frame.ACK)
                            _logger.LogDebug($"Received {frame}");
                        else
                            _logger.LogWarning($"Received {frame}");

                        // publish the frame
                        _publisher.Publish(frame);

                        // wait for next frame
                        continue;
                    }

                    if (frame is DataFrame dataFrame)
                    {
                        // dataframes must be aknowledged
                        _logger.LogDebug($"Writing {Frame.ACK}");
                        await _writer.Write(Frame.ACK, cancellation);

                        // check the type of the frame
                        switch (dataFrame.Type)
                        {
                            // response on a request
                            case DataFrameType.RES:
                                // so create and publish ResponseMessage
                                _publisher.Publish(new ResponseMessage(dataFrame.Payload));
                                break;
                            
                            // unsolicited event
                            case DataFrameType.REQ:
                                // so create and publish EventMessage
                                _publisher.Publish(new EventMessage(dataFrame.Payload));
                                break;
                        }

                        // wait for next frame
                        continue;
                    }

                }
            }, cancellation);
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
                    // create completion source, will be completed on an expected response (ACK, NAK, CAN)
                    var completion = new TaskCompletionSource<Frame>();

                    // callback, called on every frame received
                    void onVerifyResponse(Frame frame)
                    {
                        // one of the expected responses?
                        if (frame == Frame.ACK || frame == Frame.NAK || frame == Frame.CAN)
                        {
                            // yes, so set complete
                            completion.TrySetResult(frame);
                        }
                    };

                    // start listening for received frames, call onVerifyResponse for every received frame 
                    using (var subscription = _publisher.Subcribe<Frame>(onVerifyResponse))
                    {
                        var frame = new DataFrame(DataFrameType.REQ, message.Payload);

                        if (retransmissions == 0)
                            _logger.LogDebug($"Sending frame {frame}");
                        else                           
                            _logger.LogWarning($"Resending frame {frame}, attempt: {retransmissions}");

                        // send the request
                        await _writer.Write(frame, cancellation);

                        // mesasure time until frame received
                        stopwatch.Restart();
                        
                        // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 5.1 ACK frame
                        // The host MUST wait for a period of 1500ms before timing out waiting for the ACK frame
                        var timeout = Task.Delay(ProtocolSettings.ACKWaitTime, cancellation);

                        _logger.LogDebug($"Wait for ACK, NAK or CAN or timeout");

                        // wait for ACK, NAK or CAN or timeout
                        if ((await Task.WhenAny(completion.Task, timeout)) == completion.Task)
                        {
                            // response received, see what we got
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
                        else
                        {
                            // Timeout
                            
                            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.3 Retransmission
                            // A host or Z-Wave chip MUST NOT carry out more than 3 retransmissions
                            if (retransmissions >= ProtocolSettings.MaxRetryAttempts)
                                throw new TimeoutException("Timeout while waiting for an ACK");
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
            }
            finally
            {
                // done, so release lock, other send operations allowed
                _sendLock.Release();
            }
        }
    }
}
