using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ZWave4Net.CommandClasses.Services
{
    public class AssociationService : CommandClassService, IAssociation
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

        public AssociationService(ZWaveController controller, byte nodeID, byte endpointID)
            : base(CommandClass.Association, controller, nodeID, endpointID)
        {
        }

        public Task<AssociationReport> Get(byte groupID, CancellationToken cancellation = default(CancellationToken))
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "groupID must be greater than zero");

            var command = new Channel.Command(CommandClass, Command.Get, groupID);
            return Send<AssociationReport>(command, Command.Report, cancellation);
        }

        public Task Set(byte groupID, byte[] nodes, CancellationToken cancellation = default(CancellationToken))
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "groupID must be greater than zero");

            var command = new Channel.Command(CommandClass, Command.Set, (new[] { groupID }).Concat(nodes));
            return Send(command, cancellation);
        }


        public Task Remove(byte groupID, byte[] nodes, CancellationToken cancellation = default(CancellationToken))
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "groupID must be greater than zero");
            if (nodes.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(nodes), nodes, "nodes should contain at least one node");

            var command = new Channel.Command(CommandClass, Command.Remove, (new[] { groupID }).Concat(nodes));
            return Send(command, cancellation);
        }

        public Task<AssociationGroupingsReport> GroupingsGet(CancellationToken cancellation = default(CancellationToken))
        {
            var command = new Channel.Command(CommandClass, Command.GroupingsGet);
            return Send<AssociationGroupingsReport>(command, Command.GroupingsReport, cancellation);
        }
    }
}
