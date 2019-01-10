using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel;

namespace ZWave4Net.CommandClasses.Services
{
    internal class MultiChannelAssociationService : CommandClassService, IMultiChannelAssociation
    {
        private const byte MultiChannelAssociationSetMarker = 0;
        private const byte MultiChannelAssociationRemoveMarker = 0;

        enum MultiChannelAssociationCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            Remove = 0x04,
            GroupingsGet = 0x05,
            GroupingsReport = 0x06
        }

        public MultiChannelAssociationService(byte nodeID, byte endpointID, ZWaveController controller)
            : base(nodeID, endpointID, controller, CommandClass.MultiChannelAssociation)
        {
        }

        public Task<MultiChannelAssociationReport> Get(byte groupId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new Command(CommandClass, MultiChannelAssociationCommand.Get, groupId);
            return Send<MultiChannelAssociationReport>(command, MultiChannelAssociationCommand.Report, cancellationToken);
        }

        public Task<AssociationGroupingsReport> GetGroupings(CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new Command(CommandClass, MultiChannelAssociationCommand.GroupingsGet);
            return Send<AssociationGroupingsReport>(command, MultiChannelAssociationCommand.GroupingsReport, cancellationToken);
        }

        public Task Set(byte groupID, byte[] nodes, EndpointAssociation[] endpoints, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "groupID must be greater than zero");

            var payload = GetEndpointsPayload(groupID, nodes, endpoints, MultiChannelAssociationSetMarker);
            var command = new Command(CommandClass, MultiChannelAssociationCommand.Set, payload);
            return Send(command, cancellationToken);
        }

        public Task Remove(byte groupID, byte[] nodes, EndpointAssociation[] endpoints, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "groupID must be greater than zero");

            var payload = GetEndpointsPayload(groupID, nodes, endpoints, MultiChannelAssociationRemoveMarker);
            var command = new Command(CommandClass, MultiChannelAssociationCommand.Remove, payload);
            return Send(command, cancellationToken);
        }

        private static byte[] GetEndpointsPayload(byte groupID, byte[] nodes, EndpointAssociation[] endpoints, byte marker)
        {
            var payload = endpoints.SelectMany(e => new byte[] { e.NodeID, e.EndpointID });
            return new byte[] { groupID }.Concat(nodes).Concat(new[] { marker }).Concat(payload).ToArray();
        }
    }
}
