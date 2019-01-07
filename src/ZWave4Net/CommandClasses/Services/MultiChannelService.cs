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

        public MultiChannelService(byte nodeID, byte endpointID, ZWaveController controller)
            : base(nodeID, endpointID, CommandClass.MultiChannel, controller)
        {
        }

        public Task<MultiChannelEndpointsReport> GetEndpoints(CancellationToken cancellation = default(CancellationToken))
        {
            var command = new Channel.Command(CommandClass, Command.EndPointGet);
            return Send<MultiChannelEndpointsReport>(command, Command.EndPointReport, cancellation);
        }

    }
}
