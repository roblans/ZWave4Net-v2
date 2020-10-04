using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ZWave.Utilities
{
    internal class Publisher
    {
        private object _lock = new object();

        private List<ISubcriber> _subscribers = new List<ISubcriber>();

        public IDisposable Subcribe<T>(Action<T> callback)
        {
            lock (_lock)
            {
                var subcriber = new Subcriber<T>(callback, Unsubscribe);
                _subscribers.Add(subcriber);
                return subcriber;
            }
        }

        public void Publish(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var subcribers = default(ISubcriber[]);

            lock (_lock)
            {
                subcribers = _subscribers.ToArray();
            }

            foreach (var subscriber in subcribers)
            {
                subscriber.Notify(value);
            }
        }

        private void Unsubscribe(ISubcriber subcriber)
        {
            lock (_lock)
            {
                _subscribers.Remove(subcriber);
            }
        }

        interface ISubcriber : IDisposable
        {
            void Notify(object value);
        }

        class Subcriber<T> : ISubcriber
        {
            private readonly Action<T> _onNotify;
            private Action<ISubcriber> _onDispose;

            public Subcriber(Action<T> onNotify, Action<ISubcriber> onDispose)
            {
                _onNotify = onNotify ?? throw new ArgumentNullException(nameof(onNotify));
                _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
            }

            public void Notify(object value)
            {
                if (value == null)
                    return;

                if (typeof(T).IsAssignableFrom(value.GetType()))
                {
                    _onNotify((T)value);
                }
            }

            public void Dispose()
            {
                // Interlocked allows the action to be executed only once
                Interlocked.Exchange(ref _onDispose, null)?.Invoke(this);
            }
        }
    }
}