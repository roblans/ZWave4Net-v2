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
            var reply = await Controller.Channel.Send(NodeID, EndpointID, command, Convert.ToByte(responseCommand));

            // push NodeID and EndpointID in the payload so T has access to the node and the endpoint
            return new Payload(new[] { NodeID, EndpointID }.Concat(reply.Payload))
                .Deserialize<T>();
        }

        protected IObservable<T> Reports<T>(Enum command) where T : NodeReport, new()
        {
            var reportCommand = new Command(CommandClass, command);

            return Controller.Channel.ReceiveNodeEvents(NodeID, EndpointID, reportCommand)
                // push NodeID and EndpointID in the payload so T has access to the node and the endpoint
                .Select(element => new Payload(new[] { NodeID, EndpointID }.Concat(element.Payload)))
                .Select(element => element.Deserialize<T>()); 
        }
    }
}
