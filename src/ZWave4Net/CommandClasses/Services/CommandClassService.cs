using System;
using System.Linq;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;
using System.Reactive.Linq;
using System.Threading;

namespace ZWave4Net.CommandClasses.Services
{
    internal class CommandClassService
    {
        public readonly CommandClass CommandClass;
        public readonly ZWaveController Controller;
        public readonly byte NodeID;
        public readonly byte EndpointID;

        public CommandClassService(CommandClass commandClass, ZWaveController controller, byte nodeID, byte endpointID)
        {
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");

            CommandClass = commandClass;
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
            NodeID = nodeID;
            EndpointID = endpointID;
        }

        protected Task Send(Command command, CancellationToken cancellation = default(CancellationToken))
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return Controller.Channel.Send(NodeID, EndpointID, command, cancellation);
        }

        protected async Task<T> Send<T>(Command command, Enum responseCommand, CancellationToken cancellation = default(CancellationToken)) where T : Report, new()
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var reply = await Controller.Channel.Send(NodeID, EndpointID, command, Convert.ToByte(responseCommand), cancellation);

            // push NodeID and EndpointID in the payload so T has access to the node and the endpoint
            return new Payload(new[] { NodeID, EndpointID }.Concat(reply.Payload))
                .Deserialize<T>();
        }

        protected IObservable<T> Reports<T>(Enum command) where T : Report, new()
        {
            var reportCommand = new Command(CommandClass, command);

            return Controller.Channel.ReceiveNodeEvents(NodeID, EndpointID, reportCommand)
                // push NodeID and EndpointID in the payload so T has access to the node and the endpoint
                .Select(element => new Payload(new[] { NodeID, EndpointID }.Concat(element.Payload)))
                .Select(element => element.Deserialize<T>()); 
        }
    }
}
