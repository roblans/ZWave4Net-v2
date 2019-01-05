using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ZWave4Net.CommandClasses.Services
{
    internal class MultiChannelService : CommandClassService, IMultiChannel
    {
        enum Command
        {
            EndPointGet = 0x07,
            EndPointReport = 0x08,
            CapabilityGet = 0x09,
            CapabilityReport = 0x0a,
        }

        public MultiChannelService(ZWaveController controller, byte nodeID, byte endpointID)
            : base(CommandClass.MultiChannel, controller, nodeID, endpointID)
        {
        }

        public Task<MultiChannelEndpointsReport> GetEndpoints(CancellationToken cancellation = default(CancellationToken))
        {
            var command = new Channel.Command(CommandClass, Command.EndPointGet);
            return Send<MultiChannelEndpointsReport>(command, Command.EndPointReport, cancellation);
        }

    }
}
