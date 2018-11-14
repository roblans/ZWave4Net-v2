using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.Channel.Protocol
{
    public class FrameBroker
    {
        private readonly ConcurrentDictionary<Delegate, object> _subscribers = new ConcurrentDictionary<Delegate, object>();
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

        private void Unsubscribe(Delegate subscriber)
        {
            _subscribers.TryRemove(subscriber, out _);
        }

        private IDisposable Subscribe(Action<Frame> subscriber)
        {
            _subscribers.TryAdd(subscriber, null);
            return new Unsubscriber<Frame>(subscriber, (item) => Unsubscribe(item));
        }

        private Task Publish(Frame frame)
        {
            return Task.Run(() => Parallel.ForEach(_subscribers, (subscriber) =>
            {
                if (subscriber.Key is Action<Frame>)
                {
                    ((Action<Frame>)subscriber.Key)(frame);
                }
                if (subscriber.Key is Action<DataFrame> && frame is DataFrame dataFrame)
                {
                    ((Action<DataFrame>)subscriber.Key)(dataFrame);
                }
            }));
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

                    var frame = default(Frame);
                    try
                    {
                        frame = await _reader.Read(Cancelation);
                        Debug.WriteLine($"Received: {frame}");
                    }
                    catch (ChecksumException ex)
                    {
                        Debug.WriteLine(ex.Message);

                        Debug.WriteLine($"Writing: {Frame.NAK}");
                        await _writer.Write(Frame.NAK, Cancelation);

                        continue;
                    }

                    if (frame is RequestDataFrame requestDataFrame)
                    {
                        Debug.WriteLine($"Writing: {Frame.ACK}");
                        await _writer.Write(Frame.ACK, Cancelation);
                    }

                    await Publish(frame);
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

        public IDisposable Subscribe(Action<DataFrame> subscriber)
        {
            _subscribers.TryAdd(subscriber, null);
            return new Unsubscriber<DataFrame>(subscriber, (item) => Unsubscribe(item));
        }

        private class Unsubscriber<T> : IDisposable
        {
            private readonly Action<T> _subscriber;
            private readonly Action<Action<T>> _onUnsubscribe;

            public Unsubscriber(Action<T> subscriber, Action<Action<T>> onUnsubscribe)
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
