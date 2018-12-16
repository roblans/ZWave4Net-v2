using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

        public Task<AssociationReport> Get(byte groupID)
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "GroupID must be greater than zero");

            var command = new Channel.Command(CommandClass, Command.Get, groupID);
            return Send<AssociationReport>(command, Command.Report);
        }

        public Task Set(byte groupID, params byte[] nodes)
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "GroupID must be greater than zero");

            var command = new Channel.Command(CommandClass, Command.Set, (new[] { groupID }).Concat(nodes));
            return Send(command);
        }


        public Task Remove(byte groupID, params byte[] nodes)
        {
            if (groupID == 0)
                throw new ArgumentOutOfRangeException(nameof(groupID), groupID, "GroupID must be greater than zero");

            var command = new Channel.Command(CommandClass, Command.Remove, (new[] { groupID }).Concat(nodes));
            return Send(command);
        }

        public Task<AssociationGroupingsReport> GroupingsGet()
        {
            var command = new Channel.Command(CommandClass, Command.GroupingsGet);
            return Send<AssociationGroupingsReport>(command, Command.GroupingsReport);
        }
    }
}
