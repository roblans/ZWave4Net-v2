using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel;

namespace ZWave4Net.CommandClasses.Services
{
    internal class ManufacturerSpecificService : CommandClassService, IManufacturerSpecific
    {
        enum ManufacturerSpecificCommand
        {
            Get = 0x04,
            Report = 0x05
        }

        public ManufacturerSpecificService(byte nodeID, byte endpointID, ZWaveController controller)
            : base(nodeID, endpointID, controller, CommandClass.ManufacturerSpecific)
        {
        }

        public Task<ManufacturerSpecificReport> Get(CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new Command(CommandClass, ManufacturerSpecificCommand.Get);
            return Send<ManufacturerSpecificReport>(command, ManufacturerSpecificCommand.Report, cancellationToken);
        }
    }
}
