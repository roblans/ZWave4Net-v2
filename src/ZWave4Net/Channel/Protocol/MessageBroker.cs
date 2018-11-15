using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Utilities;

namespace ZWave4Net.Channel.Protocol
{
    public class MessageBroker
    {
        private readonly FrameReader _reader;
        private readonly FrameWriter _writer;
        private readonly ValueChangedEvent<Frame> _lastFrameEvent = new ValueChangedEvent<Frame>(null);
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private Task _task;

        public readonly CancellationToken Cancelation;

        public MessageBroker(IDuplexStream stream, CancellationToken cancelation)
        {
            _reader = new FrameReader(stream);
            _writer = new FrameWriter(stream);

            Cancelation = cancelation;
        }

        public void Start()
        {
            if (_task != null)
                throw new InvalidOperationException("Broker already started");

            _task = Task.Run(async () =>
            {
                while (!Cancelation.IsCancellationRequested)
                {
                    var frame = default(Frame);
                    try
                    {
                        frame = await _reader.Read(Cancelation);

                        _lastFrameEvent.Signal(frame);

                        Debug.WriteLine($"Received: {frame}");
                    }
                    catch (ChecksumException ex)
                    {
                        Debug.WriteLine(ex.Message);

                        Debug.WriteLine($"Writing: {Frame.NAK}");
                        await _writer.Write(Frame.NAK, Cancelation);

                        continue;
                    }

                    if (frame is DataFrame dataFrame)
                    {
                        Debug.WriteLine($"Writing: {Frame.ACK}");
                        await _writer.Write(Frame.ACK, Cancelation);
                    }

                }
            }, Cancelation);
        }

        public bool Stop(TimeSpan timeout)
        {
            if (_task != null)
            {
                return _task.Wait(timeout);
            }
            return true;
        }

        public async Task Send(RequestMessage message)
        {
            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.5.2 Request/Response frame flow
            // Note that due to the simple nature of the simple acknowledge mechanism, only one REQ->RES session is allowed.
            await _sendLock.WaitAsync(Cancelation);
            try
            { 
                // number of retransmissions
                var retransmissions = 0;

                while (true)
                {
                    // reset last frame receveid event
                    _lastFrameEvent.Reset();

                    // send de request
                    await _writer.Write((Frame)message, Cancelation);

                    // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 5.1 ACK frame
                    // The host MUST wait for a period of 1500ms before timing out waiting for the ACK frame
                    using (var waitCancelation = new CancellationTokenSource(1500))
                    {
                        // wait for new frame (with timeout)
                        var response = await _lastFrameEvent.Wait(waitCancelation.Token);

                        // ACK received, so where done 
                        if (response == Frame.ACK)
                            break;

                        // other options: CAN or NACK received
                        if (response == Frame.CAN || response == Frame.NAK)
                        {
                            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.3 Retransmission
                            // A host or Z-Wave chip MUST NOT carry out more than 3 retransmissions
                            if (retransmissions > 3)
                            {
                                // translate to exceptions
                                if (response == Frame.CAN)
                                    throw new CanResponseException();
                                if (response == Frame.NAK)
                                    throw new NakResponseException();
                            }

                            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.3 Retransmission
                            // Twaiting = 100ms + n*1000ms 
                            // where n is incremented at each retransmission. n = 0 is used for the first waiting period.
                            await Task.Delay(100 + retransmissions++ * 1000);
                        }
                    }
                }
            }
            finally
            {
                _sendLock.Release();
            }
        }
    }
}
