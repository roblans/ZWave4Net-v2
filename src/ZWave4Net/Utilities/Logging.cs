using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Utilities
{

    public static class Logging
    {
        private static readonly Publisher _publisher = new Publisher();

        public static IDisposable Subscribe(Action<string> action)
        {
            return _publisher.Subcribe(action);
        }

        public static void Log(string message)
        {
            var text = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t{message}";
            
            _publisher.Publish(text);
        }

        public static ILogger CreatLogger(string name)
        {
            return new Logger(name, Log);
        }

        class Logger : ILogger
        {
            private readonly string _name;
            private readonly Action<string> _onLog;

            public Logger(string name, Action<string> onLog)
            {
                _name = name;
                _onLog = onLog;
            }

            public void Log(string message)
            {
                _onLog(message);
            }

            public override string ToString()
            {
                return _name;
            }
        }
    }
    
    public interface ILogger
    {
        void Log(string message);
    }
}
