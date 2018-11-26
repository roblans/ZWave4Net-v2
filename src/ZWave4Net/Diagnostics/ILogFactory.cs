using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Diagnostics
{
    public interface ILogFactory
    {
        IDisposable Subscribe(Action<LogRecord> action);
        ILogger CreatLogger(string name);
    }
}
