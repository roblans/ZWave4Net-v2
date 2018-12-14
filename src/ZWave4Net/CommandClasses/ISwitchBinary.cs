using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    public interface ISwitchBinary
    {
        Task<SwitchBinaryReport> Get();
        Task Set(bool value);
        IObservable<SwitchBinaryReport> Reports { get; }
    }
}
