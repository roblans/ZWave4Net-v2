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
        public readonly ZWaveController Controller;
        public readonly Address Address;

        public CommandClassBase(CommandClass commandClass, ZWaveController controller, Address address)
        {
            CommandClass = commandClass;
            Controller = controller;
            Address = address;
        }

        protected Task Send(Command command)
        {
            return Controller.Channel.Send(Address, command);
        }

        protected async Task<T> Send<T>(Command command, Enum responseCommand) where T : Report, new()
        {
            var reply = await Controller.Channel.Send(Address, command, Convert.ToByte(responseCommand));

            // push NodeID and EndpointID in the payload so T has access to the node and the endpoint
            return new Payload(new[] { Address.NodeID, Address.EndpointID }.Concat(reply.Payload))
                .Deserialize<T>();
        }

        protected IObservable<T> Reports<T>(Enum command) where T : Report, new()
        {
            var reportCommand = new Command(CommandClass, command);

            return Controller.Channel.ReceiveNodeEvents(Address, reportCommand)
                // push NodeID and EndpointID in the payload so T has access to the node and the endpoint
                .Select(element => new Payload(new[] { Address.NodeID, Address.EndpointID }.Concat(element.Payload)))
                .Select(element => element.Deserialize<T>()); 
        }
    }
}
