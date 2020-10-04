using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave.Channel;

namespace ZWave.CommandClasses.Services
{
    internal class VersionService : CommandClassService, IVersion
    {
        enum VersionCommand : byte
        {
            Get = 0x11,
            Report = 0x12,
            CommandClassGet = 0x13,
            CommandClassReport = 0x14
        }

        public VersionService(byte nodeID, byte endpointID, ZWaveController controller)
            : base(nodeID, endpointID, controller, CommandClass.Version)
        {
        }

        public Task<VersionReport> Get(CancellationToken cancellationToken)
        {
            var command = new Command(CommandClass, VersionCommand.Get);
            return Send<VersionReport>(command, VersionCommand.Report, cancellationToken);
        }


        public Task<VersionCommandClassReport> GetCommandClass(CommandClass commandClass, CancellationToken cancellationToken)
        {
            var command = new Command(CommandClass, VersionCommand.CommandClassGet, Convert.ToByte(commandClass));
            return Send<VersionCommandClassReport>(command, VersionCommand.CommandClassReport, cancellationToken);
        }
    }
}
