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
        public readonly Endpoint Endpoint;
        public readonly CommandClass CommandClass;

        public CommandClassBase(Endpoint endpoint, CommandClass commandClass)
        {
            Endpoint = endpoint;
            CommandClass = commandClass;
        }

        protected Task Send(NodeCommand command)
        {
            return Endpoint.Controller.Channel.Send(Endpoint.Node.NodeID, command);
        }

        protected async Task<T> Send<T>(NodeCommand command, Enum responseCommand) where T : NodeReport, new()
        {
            var payload = await Endpoint.Node.Controller.Channel.Send<Payload>(Endpoint.Node.NodeID, command, Convert.ToByte(responseCommand));

            // push NodeID in the payload so T has access to the node
            return new Payload(new[] { Endpoint.Node.NodeID }.Concat(payload))
                .Deserialize<T>();
        }

        protected IObservable<T> Reports<T>(Enum command) where T : NodeReport, new()
        {
            return Endpoint.Node.Controller.Channel.ReceiveNodeEvents<Payload>(Endpoint.Node.NodeID, Convert.ToByte(command))
                // push NodeID in the payload so T has access to the node
                .Select(element => new Payload(new[] { Endpoint.Node.NodeID }.Concat(element)))
                .Select(element => element.Deserialize<T>()); 
        }
    }
}
