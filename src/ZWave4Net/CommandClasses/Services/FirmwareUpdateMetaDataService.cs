using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave.Channel;

namespace ZWave.CommandClasses.Services
{
    internal class FirmwareUpdateMetaDataService : CommandClassService, IFirmwareUpdateMetaData
    {
        enum FirmwareUpdateMetaDataCommand : byte
        {
            Get = 0x01,
            Report = 0x02
        }

        public FirmwareUpdateMetaDataService(byte nodeID, byte endpointID, ZWaveController controller)
            : base(nodeID, endpointID, controller, CommandClass.FirmwareUpdateMetaData)
        {
        }

        public Task<FirmwareUpdateMetaDataReport> Get(CancellationToken cancellation = default(CancellationToken))
        {
            var command = new Command(CommandClass, FirmwareUpdateMetaDataCommand.Get);
            return Send<FirmwareUpdateMetaDataReport>(command, FirmwareUpdateMetaDataCommand.Report, cancellation);
        }

    }
}
