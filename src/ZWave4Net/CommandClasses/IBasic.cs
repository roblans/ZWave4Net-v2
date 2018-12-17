using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    public interface IBasic
    {
        Task<BasicReport> Get(CancellationToken cancellation = default(CancellationToken));
        Task Set(byte value, CancellationToken cancellation = default(CancellationToken));
        IObservable<BasicReport> Reports { get; }
    }
}
