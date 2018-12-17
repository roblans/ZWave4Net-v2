using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    public interface IBinarySwitch
    {
        Task<BinarySwitchReport> Get(CancellationToken cancellation = default(CancellationToken));
        Task Set(bool value, CancellationToken cancellation = default(CancellationToken));
        IObservable<BinarySwitchReport> Reports { get; }
    }
}
