using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Diagnostics
{
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
}
