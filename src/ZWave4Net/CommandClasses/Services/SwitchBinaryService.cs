using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using System.Reactive.Linq;
using System.Threading;

namespace ZWave4Net.CommandClasses.Services
{
    internal class SwitchBinaryService : CommandClassService, ISwitchBinary
    {
        enum Command : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public SwitchBinaryService(ZWaveController controller, byte nodeID, byte endpointID)
            : base(CommandClass.SwitchBinary, controller, nodeID, endpointID)
        {
        }

        public async Task<SwitchBinaryReport> Get(CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new Channel.Command(CommandClass, Command.Get);
            return await Send<SwitchBinaryReport>(command, Command.Report, cancellationToken);
        }

        public Task Set(bool value, CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new Channel.Command(CommandClass, Command.Set, (byte)(value ? 0xFF : 0x00));
            return Send(command, cancellationToken); 
        }

        public IObservable<SwitchBinaryReport> Reports
        {
            get { return Reports<SwitchBinaryReport>(Command.Report); }
        }
    }
}
