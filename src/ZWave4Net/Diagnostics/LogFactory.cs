using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave.Utilities;

namespace ZWave.Diagnostics
{
    internal class LogFactory : ILogFactory
    {
        private readonly Publisher _publisher = new Publisher();

        public IDisposable Subscribe(Action<LogRecord> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return _publisher.Subcribe(action);
        }

        private void Log((string Category, LogLevel Level, object State) entry)
        {
            var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t{entry.Level}\t{entry.Category}\t{entry.State?.ToString()}";
            _publisher.Publish(new LogRecord(entry.Category, entry.Level, message));
        }

        public ILogger CreatLogger(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return new Logger(name, Log);
        }

        class Logger : ILogger
        {
            public readonly string Name;
            private readonly Action<(string Category, LogLevel Level, object State)> _onLog;

            public Logger(string name, Action<(string Category, LogLevel Level, object State)> onLog)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                _onLog = onLog ?? throw new ArgumentNullException(nameof(onLog));
            }

            public void Log(LogLevel level, object state)
            {
                _onLog((Name, level, state));
            }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
