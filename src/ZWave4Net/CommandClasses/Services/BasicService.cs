using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using System.Reactive.Linq;
using System.Threading;

namespace ZWave4Net.CommandClasses.Services
{
    internal class BasicService : CommandClassService, IBasic
    {
        enum BasicCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public BasicService(byte nodeID, byte endpointID, ZWaveController controller)
            : base(nodeID, endpointID, controller, CommandClass.Basic)
        {
        }

        public Task<BasicReport> Get(CancellationToken cancellation = default(CancellationToken))
        {
            var command = new Command(CommandClass, BasicCommand.Get);
            return Send<BasicReport>(command, BasicCommand.Report, cancellation);
        }

        public Task Set(byte value, CancellationToken cancellation = default(CancellationToken))
        {
            var command = new Command(CommandClass, BasicCommand.Set, value);
            return Send(command, cancellation); 
        }

        public IObservable<BasicReport> Reports
        {
            get { return Reports<BasicReport>(BasicCommand.Report); }
        }
    }
}
