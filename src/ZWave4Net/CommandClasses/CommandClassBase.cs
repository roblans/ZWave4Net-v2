using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.CommandClasses
{
    public class CommandClassBase
    {
        public readonly Node Node;
        public readonly CommandClass Class;

        public CommandClassBase(Node node, CommandClass @class)
        {
            Node = node;
            Class = @class;
        }

        protected Task Send(NodeCommand command)
        {
            return Node.Controller.Channel.Send(Node.NodeID, command);
        }

        protected Task<Payload> Send(NodeCommand command, Enum responseCommand)
        {
            return Node.Controller.Channel.Send(Node.NodeID, command, Convert.ToByte(responseCommand));
        }
    }
}
