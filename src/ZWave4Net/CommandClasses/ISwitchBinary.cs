using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    public interface ISwitchBinary
    {
        Task<SwitchBinaryReport> Get(CancellationToken cancellation = default(CancellationToken));
        Task Set(bool value, CancellationToken cancellation = default(CancellationToken));
        IObservable<SwitchBinaryReport> Reports { get; }
    }
}
