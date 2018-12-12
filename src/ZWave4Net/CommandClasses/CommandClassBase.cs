using System;
using System.Linq;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;
using System.Reactive.Linq;

namespace ZWave4Net.CommandClasses
{
    public class CommandClassBase
    {
        public readonly Node Node;
        public readonly CommandClass CommandClass;

        public CommandClassBase(Node node, CommandClass commandClass)
        {
            Node = node;
            CommandClass = commandClass;
        }

        protected Task Send(NodeCommand command)
        {
            return Node.Controller.Channel.Send(Node.NodeID, command);
        }

        protected async Task<T> Send<T>(NodeCommand command, Enum responseCommand) where T : NodeReport, new()
        {
            var payload = await Node.Controller.Channel.Send<Payload>(Node.NodeID, command, Convert.ToByte(responseCommand));
            return new Payload(new[] { Node.NodeID }.Concat(payload.ToArray()).ToArray()).Deserialize<T>();
        }

        protected IObservable<T> Reports<T>(Enum command) where T : NodeReport, new()
        {
            return Node.Controller.Channel.NodeEvents<Payload>(Node.NodeID, Convert.ToByte(command))
                .Select(element => new Payload(new[] { Node.NodeID }.Concat(element.ToArray()).ToArray()))
                .Select(element => element.Deserialize<T>()); 
        }
    }
}
