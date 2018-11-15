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
            while (true)
            {
                _lastFrameEvent.Reset();

                await _writer.Write((Frame)message, Cancelation);

                var response = await _lastFrameEvent.Wait();
                if (response == Frame.ACK)
                    break;
            }
        }
    }
}
