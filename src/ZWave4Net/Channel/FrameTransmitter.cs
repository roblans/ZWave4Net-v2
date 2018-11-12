using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class FrameTransmitter
    {
        private readonly BlockingCollection<Frame> _queue = new BlockingCollection<Frame>();
        private Task _task;
        public readonly FrameWriter Writer;
        public readonly CancellationToken Cancelation;

        public FrameTransmitter(FrameWriter writer, CancellationToken cancelation)
        {
            Writer = writer;
            Cancelation = cancelation;
        }

        public void Start()
        {
            if (_task != null)
                throw new InvalidOperationException("Transmitter already started");

            _task = Task.Run(async () =>
            {
                foreach(var frame in _queue.GetConsumingEnumerable(Cancelation))
                {
                    await Writer.Write(frame, Cancelation);
                }
            }, Cancelation);
        }

        public void Transmit(Frame frame)
        {
            _queue.Add(frame);
        }

        public bool Wait(TimeSpan timeout)
        {
            if (_task != null)
            {
                return _task.Wait(timeout);
            }
            return true;
        }
    }
}
