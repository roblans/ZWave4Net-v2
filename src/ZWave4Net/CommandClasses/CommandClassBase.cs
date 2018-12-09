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

        protected Task<T> Send<T>(NodeCommand command, Enum responseCommand) where T : IPayloadSerializable, new()
        {
            return Node.Controller.Channel.Send<T>(Node.NodeID, command, Convert.ToByte(responseCommand));
        }
    }
}
