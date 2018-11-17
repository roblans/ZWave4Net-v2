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

namespace ZWave4Net.Channel.Protocol
{
    public class MessageBroker
    {
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

        public void Run(CancellationToken cancelation)
        {
            if (_task != null)
                throw new InvalidOperationException("Broker already running");

            _task = Task.Run(async () =>
            {
                while (!cancelation.IsCancellationRequested)
                {
                    var frame = default(Frame);
                    try
                    {
                        frame = await _reader.Read(cancelation);

                        Debug.WriteLine($"Received: {frame}");

                        _publisher.Publish(frame);
                    }
                    catch (ChecksumException ex)
                    {
                        Debug.WriteLine(ex.Message);

                        Debug.WriteLine($"Writing: {Frame.NAK}");
                        await _writer.Write(Frame.NAK, cancelation);

                        continue;
                    }

                    if (frame is DataFrame dataFrame)
                    {
                        Debug.WriteLine($"Writing: {Frame.ACK}");
                        await _writer.Write(Frame.ACK, cancelation);

                        switch (dataFrame.Type)
                        {
                            case DataFrameType.RES:
                                _publisher.Publish(new ResponseMessage((ControllerFunction)dataFrame.Payload[0], dataFrame.Payload.Skip(1).ToArray()));
                                break;
                            case DataFrameType.REQ:
                                _publisher.Publish(new EventMessage((ControllerFunction)dataFrame.Payload[0], dataFrame.Payload.Skip(1).ToArray()));
                                break;
                        }
                    }

                }
            }, cancelation);
        }

        public async Task Send(RequestMessage message, CancellationToken cancelation)
        {
            var stopwatch = Stopwatch.StartNew();

            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.5.2 Request/Response frame flow
            // Note that due to the simple nature of the simple acknowledge mechanism, only one REQ->RES session is allowed.
            await _sendLock.WaitAsync(cancelation);
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
                        if (retransmissions == 0)
                            Debug.WriteLine($"Sending frame");
                        else                           
                            Debug.WriteLine($"Sending frame, retransmission: {retransmissions}");

                        // send the request
                        await _writer.Write(new DataFrame(DataFrameType.REQ, message.Payload), cancelation);

                        // mesasure time until frame received
                        stopwatch.Restart();
                        
                        // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 5.1 ACK frame
                        // The host MUST wait for a period of 1500ms before timing out waiting for the ACK frame
                        var timeout = Task.Delay(SerialProtocol.ACKWaitTime, cancelation);

                        Debug.WriteLine($"Wait for ACK, NAK or CAN or timeout");

                        // wait for ACK, NAK or CAN or timeout
                        if ((await Task.WhenAny(completion.Task, timeout)) == completion.Task)
                        {
                            // response received, see what we got
                            var response = await completion.Task;

                            Debug.WriteLine($"{response} received");

                            // ACK received, so where done 
                            if (response == Frame.ACK)
                                break;

                            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.3 Retransmission
                            // A host or Z-Wave chip MUST NOT carry out more than 3 retransmissions
                            if (retransmissions >= SerialProtocol.MaxRetryAttempts)
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
                            if (retransmissions >= SerialProtocol.MaxRetryAttempts)
                                throw new TimeoutException("Timeout while waiting for an ACK");
                        }


                        // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.3 Retransmission
                        // Twaiting = 100ms + n*1000ms 
                        // where n is incremented at each retransmission. n = 0 is used for the first waiting period.
                        var waitTime = SerialProtocol.RetryDelayWaitTime.TotalMilliseconds + (retransmissions++ * SerialProtocol.RetryAttemptWaitTime.TotalMilliseconds);

                        // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.2.2 Data frame delivery timeout
                        // The transmitter MAY compensate for the 1600ms already elapsed when calculating the retransmission waiting period
                        waitTime -= stopwatch.ElapsedMilliseconds;
                        if (waitTime > 0)
                        {
                            await Task.Delay((int)waitTime, cancelation);
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
