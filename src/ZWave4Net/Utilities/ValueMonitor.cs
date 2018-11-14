using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.Utilities
{
    public class ValueMonitor<T>
    {
        private readonly object _lock = new object();
        private T _currentValue;
        private TaskCompletionSource<T> _completion;

        public readonly T InitialValue;

        public ValueMonitor(T initialValue)
        {
            InitialValue = initialValue;
        }

        public void ResetValue()
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

        public void UpdateValue(T value)
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

        public Task<T> WaitForUpdate()
        {
            lock (_lock)
            {
                return _completion.Task;
            }
        }

        public async Task<T> WaitForUpdate(CancellationToken cancellationToken)
        {
            var waitTask = WaitForUpdate();

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