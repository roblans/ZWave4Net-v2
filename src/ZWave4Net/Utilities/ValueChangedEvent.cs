using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.Utilities
{
    public class ValueChangedEvent<T>
    {
        private readonly object _lock = new object();
        private T _currentValue;
        private TaskCompletionSource<T> _completion;

        public readonly T InitialValue;

        public ValueChangedEvent(T initialValue)
        {
            InitialValue = initialValue;
        }

        public void Reset()
        {
            lock (_lock)
            {
                if (_completion == null || _completion.Task.IsCompleted)
                {
                    _completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                    _currentValue = InitialValue;
                }
            }
        }

        public void Signal(T value)
        {
            lock (_lock)
            {
                if (!object.Equals(_currentValue, value))
                {
                    _currentValue = value;
                    _completion.TrySetResult(_currentValue);
                }
            }
        }

        public Task<T> Wait()
        {
            lock (_lock)
            {
                return _completion.Task;
            }
        }

        public async Task<T> Wait(CancellationToken cancellationToken)
        {
            var waitTask = Wait();

            if (waitTask.IsCompleted)
                await waitTask;

            if (!cancellationToken.CanBeCanceled)
                await waitTask;

            if (cancellationToken.IsCancellationRequested)
                return await Task.FromCanceled<T>(cancellationToken);

            using (cancellationToken.Register(() => _completion.SetCanceled()))
            {
                return await waitTask;
            }
        }
    }
}