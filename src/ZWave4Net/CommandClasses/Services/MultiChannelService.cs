using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ZWave4Net.Channel;

namespace ZWave4Net.CommandClasses.Services
{
    internal class MultiChannelService : CommandClassService, IMultiChannel
    {
        enum MultiChannelCommand
        {
            EndPointGet = 0x07,
            EndPointReport = 0x08,
            CapabilityGet = 0x09,
            CapabilityReport = 0x0a,
        }

        public MultiChannelService(byte nodeID, byte endpointID, ZWaveController controller)
            : base(nodeID, endpointID, CommandClass.MultiChannel, controller)
        {
        }

        public Task<MultiChannelEndpointsReport> GetEndpoints(CancellationToken cancellation = default(CancellationToken))
        {
            var command = new Command(CommandClass, MultiChannelCommand.EndPointGet);
            return Send<MultiChannelEndpointsReport>(command, MultiChannelCommand.EndPointReport, cancellation);
        }

    }
}
