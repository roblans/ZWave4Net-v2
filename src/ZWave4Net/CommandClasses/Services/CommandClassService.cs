using System;
using System.Linq;
using System.Threading.Tasks;
using ZWave.Channel;
using ZWave.Channel.Protocol;
using System.Reactive.Linq;
using System.Threading;

namespace ZWave.CommandClasses.Services
{
    internal abstract class CommandClassService
    {
        private readonly byte _nodeID;
        private readonly byte _endpointID;

        private Node _node;
        private Endpoint _endpoint;

        public readonly ZWaveController Controller;
        public readonly CommandClass CommandClass;

        public CommandClassService(byte nodeID, byte endpointID, ZWaveController controller, CommandClass commandClass)
        {
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");

            _nodeID = nodeID;
            _endpointID = endpointID;
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
            CommandClass = commandClass;
        }

        public Node Node
        {
            get { return _node ?? (_node = Controller.Nodes[_nodeID]); }
        }

        public Endpoint Endpoint
        {
            get { return _endpoint ?? (_endpoint = Node.Endpoints[_endpointID]); }
        }

        private Command CreateTransportCommand(Command command)
        {
            // addressing an enpoint?
            if (_endpointID != 0)
            {
                // yes, so wrap command in a encapsulated multi channel command
                command = MultiChannelCommand.Encapsulate(0, _endpointID, command);
            }
            // additional CRC16 checksum required?
            if (Node.UseCrc16Checksum)
            {
                // yes, so wrap command in a encapsulated CRC16 endcap command
                command = Crc16Command.Encapsulate(command);
            }
            return command;
        }

        protected Task Send(Command command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var transportCommand = CreateTransportCommand(command);

            return Controller.Channel.Send(Node.NodeID, transportCommand, cancellationToken);
        }

        protected async Task<T> Send<T>(Command command, Enum responseCommand, CancellationToken cancellationToken = default(CancellationToken)) where T : Report, new()
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var transportCommand = CreateTransportCommand(command);

            var reply = await Controller.Channel.Send(Node.NodeID, transportCommand, Convert.ToByte(responseCommand), cancellationToken);
            return Report.Create<T>(Node, Endpoint, reply.Payload);
        }

        protected IObservable<T> Reports<T>(Enum command) where T : Report, new()
        {
            var receiveCommand = new Command(CommandClass, command);
            var transportCommand = CreateTransportCommand(receiveCommand);

            return Controller.Channel.ReceiveNodeEvents(Node.NodeID, transportCommand)
                .Select(@event => Report.Create<T>(Node, Endpoint, @event.Payload));
        }
    }
}
