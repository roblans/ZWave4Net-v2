using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel;

namespace ZWave4Net.CommandClasses.Services
{
    internal class PowerlevelService : CommandClassService, IPowerlevel
    {
        enum PowerlevelCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public PowerlevelService(byte nodeID, byte endpointID, ZWaveController controller)
            : base(nodeID, endpointID, CommandClass.Powerlevel, controller)
        {
        }

        public Task<PowerlevelReport> Get(CancellationToken cancellation = default(CancellationToken))
        {
            var command = new Command(CommandClass, PowerlevelCommand.Get);
            return Send<PowerlevelReport>(command, PowerlevelCommand.Report, cancellation);
        }

        public Task Set(Powerlevel Level, TimeSpan timeout, CancellationToken cancellation = default(CancellationToken))
        {
            if (timeout.TotalSeconds < 1 || timeout.TotalSeconds > 255)
                throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "Timout must be between 1 and 255 seconds");

            var command = new Command(CommandClass, PowerlevelCommand.Set, Convert.ToByte(Level), (byte)timeout.TotalSeconds);
            return Send(command, cancellation);
        }

    }
}
