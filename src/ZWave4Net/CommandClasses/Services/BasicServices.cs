using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using System.Reactive.Linq;
using System.Threading;

namespace ZWave4Net.CommandClasses.Services
{
    internal class BasicServices : CommandClassService, IBasic
    {
        enum Command : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public BasicServices(ZWaveController controller, byte nodeID, byte endpointID)
            : base(CommandClass.Basic, controller, nodeID, endpointID)
        {
        }

        public Task<BasicReport> Get(CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new Channel.Command(CommandClass, Command.Get);
            return Send<BasicReport>(command, Command.Report, cancellationToken);
        }

        public Task Set(byte value, CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new Channel.Command(CommandClass, Command.Set, value);
            return Send(command, cancellationToken); 
        }

        public IObservable<BasicReport> Reports
        {
            get { return Reports<BasicReport>(Command.Report); }
        }
    }
}
