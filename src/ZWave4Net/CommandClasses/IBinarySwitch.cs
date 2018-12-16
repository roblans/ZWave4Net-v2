using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    public interface IBinarySwitch
    {
        Task<BinarySwitchReport> Get();
        Task Set(bool value);
        IObservable<BinarySwitchReport> Reports { get; }
    }
}
