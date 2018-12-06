using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Reactive.Subjects;

namespace ZWave4Net.Utilities
{
    public abstract class Receiver<T> : IObservable<T>
    {
        private readonly IObservable<T> _observable;
        private Task _task;

        public Receiver()
        {
            _observable = Observable.Create((Func<IObserver<T>, CancellationToken, Task>)Subscribe).Publish().RefCount();
        }

        private Task Subscribe(IObserver<T> observer, CancellationToken cancellation)
        {
            return _task = Execute(observer, cancellation);
        }

        protected abstract Task Execute(IObserver<T> observer, CancellationToken cancellation);

        public TaskAwaiter GetAwaiter()
        {
            return _task?.GetAwaiter() ?? default(TaskAwaiter);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}
