using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ZWave.Channel;

namespace ZWave.CommandClasses.Services
{
    internal class AssociationService : CommandClassService, IAssociation
    {
        enum AssociationCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            Remove = 0x04,
            GroupingsGet = 0x05,
            GroupingsReport = 0x06
        }

        public AssociationService(byte nodeID, byte endpointID, ZWaveController controller)
            : base(nodeID, endpointID, controller, CommandClass.Association)
        {
        }

        public Task<AssociationReport> Get(byte groupID, CancellationToken cancellationToken = default)
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "groupID must be greater than zero");

            var command = new Command(CommandClass, AssociationCommand.Get, groupID);
            return Send<AssociationReport>(command, AssociationCommand.Report, cancellationToken);
        }

        public Task Set(byte groupID, byte[] nodes, CancellationToken cancellationToken = default)
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "groupID must be greater than zero");

            var command = new Command(CommandClass, AssociationCommand.Set, (new[] { groupID }).Concat(nodes));
            return Send(command, cancellationToken);
        }


        public Task Remove(byte groupID, byte[] nodes, CancellationToken cancellationToken = default)
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "groupID must be greater than zero");
            if (nodes.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(nodes), nodes, "nodes should contain at least one node");

            var command = new Command(CommandClass, AssociationCommand.Remove, (new[] { groupID }).Concat(nodes));
            return Send(command, cancellationToken);
        }

        public Task<AssociationGroupingsReport> GetGroupings(CancellationToken cancellationToken = default)
        {
            var command = new Command(CommandClass, AssociationCommand.GroupingsGet);
            return Send<AssociationGroupingsReport>(command, AssociationCommand.GroupingsReport, cancellationToken);
        }
    }
}
