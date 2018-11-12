using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class FrameReceiver : IObservable<Frame>
    {
        private readonly ConcurrentBag<IObserver<Frame>> _observers = new ConcurrentBag<IObserver<Frame>>();
        private Task _task; 
        public readonly FrameReader Reader;
        public readonly CancellationToken Cancelation;

        public FrameReceiver(FrameReader reader, CancellationToken cancelation)
        {
            Reader = reader;
            Cancelation = cancelation;
        }

        private Task Publish(Action<IObserver<Frame>> action)
        {
            return Task.Run(() => Parallel.ForEach(_observers, (observer) => action(observer)));
        }

        public void Start()
        {
            if (_task != null)
                throw new InvalidOperationException("Receiver already started");

            _task = Task.Run(async () =>
            {
                while (!Cancelation.IsCancellationRequested)
                {
                    try
                    {
                        var frame = await Reader.Read(Cancelation);
                        if (frame == null)
                            break;

                        await Publish((observer) => observer.OnNext(frame));
                    }
                    catch(Exception ex)
                    {
                        await Publish((observer) => observer.OnError(ex));
                    }
                }
            }, Cancelation);
        }

        public bool Wait(TimeSpan timeout)
        {
            if (_task != null)
            {
                return _task.Wait(timeout);
            }
            return true;
        }

        private void Unsubscribe(IObserver<Frame> observer)
        {
            _observers.Add(observer);
        }

        public IDisposable Subscribe(IObserver<Frame> observer)
        {
            return new Unsubscriber(observer, (item) => Unsubscribe(item));
        }

        private class Unsubscriber : IDisposable
        {
            private readonly IObserver<Frame> _observer;
            private readonly Action<IObserver<Frame>> _onUnsubscribe;

            public Unsubscriber(IObserver<Frame> observer, Action<IObserver<Frame>> onUnsubscribe)
            {
                _observer = observer;
                _onUnsubscribe = onUnsubscribe;
            }

            public void Dispose()
            {
                _onUnsubscribe(_observer);
            }
        }
    }
}
