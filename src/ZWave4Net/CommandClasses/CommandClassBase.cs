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
        public readonly CommandClass CommandClass;
        public Endpoint Endpoint { get; private set; }

        public CommandClassBase(CommandClass commandClass)
        {
            CommandClass = commandClass;
        }

        public void Initialize(Endpoint endpoint)
        {
            Endpoint = endpoint;
        }

        protected Task Send(EndpointCommand command)
        {
            return Endpoint.Controller.Channel.Send(Endpoint, command);
        }

        protected async Task<T> Send<T>(EndpointCommand command, Enum responseCommand) where T : NodeReport, new()
        {
            var payload = await Endpoint.Node.Controller.Channel.Send<Payload>(Endpoint, command, Convert.ToByte(responseCommand));

            // push NodeID in the payload so T has access to the node
            return new Payload(new[] { Endpoint.Node.NodeID }.Concat(payload))
                .Deserialize<T>();
        }

        protected IObservable<T> Reports<T>(Enum command) where T : NodeReport, new()
        {
            return Endpoint.Node.Controller.Channel.ReceiveNodeEvents<Payload>(Endpoint, Convert.ToByte(command))
                // push NodeID in the payload so T has access to the node
                .Select(element => new Payload(new[] { Endpoint.Node.NodeID }.Concat(element)))
                .Select(element => element.Deserialize<T>()); 
        }
    }
}
