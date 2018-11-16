using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ZWave4Net.Utilities
{
    public class Publisher
    {
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private List<Subcription> _subscribers = new List<Subcription>();

        public IDisposable Subscibe<T>(Action<T> callback)
        {
            _lock.EnterWriteLock();
            try
            {
                var subscription = new Subcription<T>(callback, Unsubscribe);
                _subscribers.Add(subscription);
                return subscription;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Publish(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var subcribers = default(Subcription[]);

            _lock.EnterReadLock();
            try
            {
                subcribers = _subscribers.ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }

            foreach (var subscriber in _subscribers)
            {
                subscriber.Handle(value);
            }
        }

        private void Unsubscribe(Subcription subcription)
        {
            _lock.EnterWriteLock();
            try
            {
                _subscribers.Remove(subcription);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        abstract class Subcription : IDisposable
        {
            private readonly Action<Subcription> _onDispose;

            public Subcription(Action<Subcription> onDispose)
            {
                _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
            }

            public abstract void Handle(object value);

            public void Dispose()
            {
                _onDispose(this);
            }
        }

        class Subcription<T> : Subcription
        {
            private readonly Action<T> _onCallback;

            public Subcription(Action<T> onCallback, Action<Subcription> onDispose) : base(onDispose)
            {
                _onCallback = onCallback ?? throw new ArgumentNullException(nameof(onCallback));
            }

            public override void Handle(object value)
            {
                if (value == null)
                    return;

                if (typeof(T).IsAssignableFrom(value.GetType()))
                {
                    _onCallback((T)value);
                }
            }
        }
    }
}