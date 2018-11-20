using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Utilities
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical,
    }

    public class LogRecord
    {
        public readonly string Category;
        public readonly LogLevel Level;
        public readonly string Message;

        public LogRecord(string category, LogLevel level, string message)
        {
            Category = category;
            Level = level;
            Message = message;
        }

        public override string ToString()
        {
            return Message;
        }
    }

    public static class Logging
    {
        private static readonly Publisher _publisher = new Publisher();

        public static IDisposable Subscribe(Action<LogRecord> action)
        {
            return _publisher.Subcribe(action);
        }

        private static void Log(LogRecord record)
        {
            var formattedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t{record.Level}\t{record.Category}\t{record.Message}";
            _publisher.Publish(new LogRecord(record.Category, record.Level, formattedMessage));
        }

        public static ILogger CreatLogger(string name)
        {
            return new Logger(name, Log);
        }

        class Logger : ILogger
        {
            private readonly string _name;
            private readonly Action<LogRecord> _onLog;

            public Logger(string name, Action<LogRecord> onLog)
            {
                _name = name;
                _onLog = onLog;
            }

            public void Log(LogLevel level, string message)
            {
                _onLog(new LogRecord(_name, level, message));
            }

            public override string ToString()
            {
                return _name;
            }
        }
    }
    
    public interface ILogger
    {
        void Log(LogLevel level, string message);
    }

    public static partial class Extentions
    {
        public static void LogDebug(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Debug, message);
        }

        public static void LogInfo(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Info, message);
        }

        public static void LogWarning(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Warning, message);
        }

        public static void LogError(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Error, message);
        }

        public static void LogCritical(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Critical, message);
        }
    }
}
