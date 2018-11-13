using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.Channel.Protocol
{
    public class FrameBroker
    {
        private readonly ConcurrentDictionary<Action<Frame>, object> _subscribers = new ConcurrentDictionary<Action<Frame>, object>();
        private FrameReader _reader;
        private FrameWriter _writer;
        private Task _task;
        public readonly CancellationToken Cancelation;

        public FrameBroker(IByteStream stream, CancellationToken cancelation)
        {
            _reader = new FrameReader(stream);
            _writer = new FrameWriter(stream);

            Cancelation = cancelation;
        }

        private Task Publish(Action<Action<Frame>> action)
        {
            return Task.Run(() => Parallel.ForEach(_subscribers, (subscriber) => action(subscriber.Key)));
        }

        public void Start()
        {
            if (_task != null)
                throw new InvalidOperationException("Broker already started");

            _task = Task.Run(async () =>
            {
                while (true)
                {
                    Cancelation.ThrowIfCancellationRequested();

                    var frame = await _reader.Read(Cancelation);

                    if (frame is RequestDataFrame requestDataFrame)
                    {
                        await _writer.Write(Frame.ACK, Cancelation);
                    }

                    await Publish((subscriber) => subscriber(frame));
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

        public async Task Send(RequestDataFrame request)
        {
            var completion = new TaskCompletionSource<bool>();

            Cancelation.Register(() => completion.TrySetCanceled());

            var subscriber = default(IDisposable);
            subscriber = Subscribe((response) =>
            {
                if (response == Frame.ACK)
                {
                    completion.TrySetResult(true);
                    subscriber.Dispose();
                }
            });

            await _writer.Write(request, Cancelation);

            await completion.Task;
        }

        private void Unsubscribe(Action<Frame> subscriber)
        {
            _subscribers.TryRemove(subscriber, out _);
        }

        public IDisposable Subscribe(Action<Frame> subscriber)
        {
            _subscribers.TryAdd(subscriber, null);
            return new Unsubscriber(subscriber, (item) => Unsubscribe(item));
        }

        private class Unsubscriber : IDisposable
        {
            private readonly Action<Frame> _subscriber;
            private readonly Action<Action<Frame>> _onUnsubscribe;

            public Unsubscriber(Action<Frame> subscriber, Action<Action<Frame>> onUnsubscribe)
            {
                _subscriber = subscriber;
                _onUnsubscribe = onUnsubscribe;
            }

            public void Dispose()
            {
                _onUnsubscribe(_subscriber);
            }
        }
    }
}
