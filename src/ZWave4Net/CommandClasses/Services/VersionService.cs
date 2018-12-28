using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses.Services
{
    internal class VersionService : CommandClassService, IVersion
    {
        enum Command : byte
        {
            Get = 0x11,
            Report = 0x12,
            CommandClassGet = 0x13,
            CommandClassReport = 0x14
        }

        public VersionService(ZWaveController controller, byte nodeID, byte endpointID)
            : base(CommandClass.Version, controller, nodeID, endpointID)
        {
        }

        public Task<VersionReport> Get(CancellationToken cancellationToken)
        {
            var command = new Channel.Command(CommandClass, Command.Get);
            return Send<VersionReport>(command, Command.Report, cancellationToken);
        }


        public Task<VersionCommandClassReport> CommandClassGet(CommandClass commandClass, CancellationToken cancellationToken)
        {
            var command = new Channel.Command(CommandClass, Command.CommandClassGet, Convert.ToByte(commandClass));
            return Send<VersionCommandClassReport>(command, Command.CommandClassReport, cancellationToken);
        }
    }
}
