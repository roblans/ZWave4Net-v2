using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ZWave4Net.CommandClasses.Services
{
    internal class AssociationService : CommandClassService, IAssociation
    {
        enum Command
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            Remove = 0x04,
            GroupingsGet = 0x05,
            GroupingsReport = 0x06
        }

        public AssociationService(byte nodeID, byte endpointID, ZWaveController controller)
            : base(nodeID, endpointID, CommandClass.Association, controller)
        {
        }

        public Task<AssociationReport> Get(byte groupID, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "groupID must be greater than zero");

            var command = new Channel.Command(CommandClass, Command.Get, groupID);
            return Send<AssociationReport>(command, Command.Report, cancellationToken);
        }

        public Task Set(byte groupID, byte[] nodes, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "groupID must be greater than zero");

            var command = new Channel.Command(CommandClass, Command.Set, (new[] { groupID }).Concat(nodes));
            return Send(command, cancellationToken);
        }


        public Task Remove(byte groupID, byte[] nodes, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "groupID must be greater than zero");
            if (nodes.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(nodes), nodes, "nodes should contain at least one node");

            var command = new Channel.Command(CommandClass, Command.Remove, (new[] { groupID }).Concat(nodes));
            return Send(command, cancellationToken);
        }

        public Task<AssociationGroupingsReport> GetGroupings(CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new Channel.Command(CommandClass, Command.GroupingsGet);
            return Send<AssociationGroupingsReport>(command, Command.GroupingsReport, cancellationToken);
        }
    }
}
