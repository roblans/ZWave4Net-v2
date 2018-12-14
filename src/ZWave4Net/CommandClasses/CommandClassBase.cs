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
        public readonly byte NodeID;
        public readonly byte EndpointID;

        public CommandClassBase(CommandClass commandClass, ZWaveController controller, byte nodeID, byte endpointID)
        {
            CommandClass = commandClass;
            Controller = controller;
            NodeID = nodeID;
            EndpointID = endpointID;
        }

        protected Task Send(Command command)
        {
            return Controller.Channel.Send(NodeID, EndpointID, command);
        }

        protected async Task<T> Send<T>(Command command, Enum responseCommand) where T : NodeReport, new()
        {
            var payload = await Controller.Channel.Send<Payload>(NodeID, command, Convert.ToByte(responseCommand));

            // push NodeID in the payload so T has access to the node
            return new Payload(new[] { NodeID }.Concat(payload))
                .Deserialize<T>();
        }

        protected IObservable<T> Reports<T>(Enum command) where T : NodeReport, new()
        {
            return Controller.Channel.ReceiveNodeEvents<Payload>(NodeID, Convert.ToByte(command))
                // push NodeID in the payload so T has access to the node
                .Select(element => new Payload(new[] { NodeID }.Concat(element)))
                .Select(element => element.Deserialize<T>()); 
        }
    }
}
