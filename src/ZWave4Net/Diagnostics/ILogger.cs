using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Diagnostics
{
    public interface ILogger
    {
        void Log(LogLevel level, object state);
    }
}
