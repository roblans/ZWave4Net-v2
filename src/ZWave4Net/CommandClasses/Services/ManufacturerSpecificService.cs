using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses.Services
{
    internal class ManufacturerSpecificService : CommandClassService, IManufacturerSpecific
    {
        enum Command
        {
            Get = 0x04,
            Report = 0x05
        }

        public ManufacturerSpecificService(ZWaveController controller, byte nodeID, byte endpointID)
            : base(CommandClass.ManufacturerSpecific, controller, nodeID, endpointID)
        {
        }

        public Task<ManufacturerSpecificReport> Get(CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new Channel.Command(CommandClass, Command.Get);
            return Send<ManufacturerSpecificReport>(command, Command.Report, cancellationToken);
        }
    }
}
