using System;
using System.Linq;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;
using System.Reactive.Linq;
using System.Threading;

namespace ZWave4Net.CommandClasses.Services
{
    internal abstract class CommandClassService
    {
        private readonly byte _nodeID;
        private readonly byte _endpointID;
        private Node _node;
        private Endpoint _endpoint;
        public readonly CommandClass CommandClass;
        public readonly ZWaveController Controller;

        public CommandClassService(byte nodeID, byte endpointID, CommandClass commandClass, ZWaveController controller)
        {
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");

            _nodeID = nodeID;
            _endpointID = endpointID;

            CommandClass = commandClass;
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public Node Node
        {
            get { return _node ?? (_node = Controller.Nodes[_nodeID]); }
        }

        public Endpoint Endpoint
        {
            get { return _endpoint ?? (_endpoint = Node.Endpoints[_endpointID]); }
        }

        protected Task Send(Command command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            command.Crc16Checksum = Node.UseCrc16Checksum;

            return Controller.Channel.Send(Node.NodeID, Endpoint.EndpointID, command, cancellationToken);
        }

        protected async Task<T> Send<T>(Command command, Enum responseCommand, CancellationToken cancellationToken = default(CancellationToken)) where T : Report, new()
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            command.Crc16Checksum = Node.UseCrc16Checksum;

            var reply = await Controller.Channel.Send(Node.NodeID, Endpoint.EndpointID, command, Convert.ToByte(responseCommand), cancellationToken);
            return CreateReport<T>(reply.Payload);
        }

        protected IObservable<T> Reports<T>(Enum command) where T : Report, new()
        {
            var reportCommand = new Command(CommandClass, command);

            return Controller.Channel.ReceiveNodeEvents(Node.NodeID, Endpoint.EndpointID, reportCommand)
                .Select(@event => CreateReport<T>(@event.Payload));
        }

        private T CreateReport<T>(Payload payload) where T : Report, new()
        {
            var report = new T();
            report.Build(Node, Endpoint, payload);
            return report;
        }
    }
}
