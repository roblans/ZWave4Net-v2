using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.CommandClasses
{
    public class CommandClass
    {
        public readonly Node Node;

        public CommandClass(Node node)
        {
            Node = node;
        }

        protected Task<Payload> Send(NodeCommand command)
        {
            return Send<Payload>(command);
        }

        protected Task<T> Send<T>(NodeCommand command) where T : IPayload, new()
        {
            return Node.Controller.Channel.Send<T>(Node.NodeID, command);
        }
    }
}
