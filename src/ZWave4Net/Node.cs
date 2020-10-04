using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave.Channel;
using ZWave.Channel.Protocol;
using System.Reactive.Linq;

namespace ZWave
{
    /// <summary>
    /// An individual node in a network
    /// </summary>
    public class Node : Endpoint, IEquatable<Node>
    {
        /// <summary>
        /// The type of the node
        /// </summary>
        public NodeType NodeType { get; private set; }
        
        /// <summary>
        /// The security of the node
        /// </summary>
        public Security Security { get; private set; }

        /// <summary>
        /// Z-Wave devices that are plugged in to power and keep their receiver on all the time. Listening devices act as repeaters and therefore extend the Z-Wave mesh network
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// The collection of endpoints for this node
        /// </summary>
        public readonly EndpointCollection Endpoints;

        /// <summary>
        /// gets or sets a value indicating whether to include a CRC16 checksum during communication to ensure integrity of the payload,
        /// use only on nodes that supports the CRC-16 Encapsulation Command Class 
        /// </summary>
        public bool UseCrc16Checksum { get; set; }

        internal Node(byte nodeID, ZWaveController controller) : base(nodeID, 0, controller)
        {
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            Endpoints = new EndpointCollection(this);
        }

        internal async Task Initialize()
        {
            var command = new ControllerRequest(Function.GetNodeProtocolInfo, new Payload(NodeID));
            var protocolInfo = await Channel.Send<NodeProtocolInfo>(command, CancellationToken.None);

            NodeType = protocolInfo.NodeType;
            Security = protocolInfo.Security;
            IsListening = protocolInfo.IsListening;
        }

        /// <summary>
        /// Gets a value indicating whether this node is a controller 
        /// </summary>
        public bool IsController
        {
            get { return NodeID == Controller.NodeID; }
        }

        internal Endpoint CreateEndpoint(byte endpointID)
        {
            return Factory.CreateEndpoint(NodeID, endpointID, Controller);
        }

        /// <summary>
        /// Gets the node information
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<NodeInfo> GetNodeInfo(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Channel.SendRequestNodeInfo(NodeID, cancellationToken);
        }

        /// <summary>
        /// Gets the neighbours of the node
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<Node[]> GetNeighbours(CancellationToken cancellationToken = default(CancellationToken))
        {
            var results = new List<Node>();

            var command = new ControllerRequest(Function.GetRoutingTableLine, new Payload(NodeID));

            // send request
            var response = await Channel.Send<Payload>(command, cancellationToken);

            var bits = new BitArray(response.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                {
                    results.Add(Controller.Nodes[(byte)(i + 1)]);
                }
            }
            return results.ToArray();
        }

        /// <summary>
        /// Request the node to update it's neighbors
        /// </summary>
        /// <param name="progress">A callback delegate to report progress</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<NeighborUpdateStatus> RequestNeighborUpdate(IProgress<NeighborUpdateStatus> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Channel.SendRequestNeighborUpdate(NodeID, progress, cancellationToken);
        }

        /// <summary>
        /// Advertises updates of the node
        /// </summary>
        public IObservable<NodeUpdate> Updates
        {
            get { return Channel.ReceiveNodeUpdates(NodeID); }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Node);
        }

        public bool Equals(Node other)
        {
            return other != null &&  NodeID == other.NodeID;
        }

        public override int GetHashCode()
        {
            return -1960697928 + NodeID.GetHashCode();
        }

        public static bool operator ==(Node node1, Node node2)
        {
            return EqualityComparer<Node>.Default.Equals(node1, node2);
        }

        public static bool operator !=(Node node1, Node node2)
        {
            return !(node1 == node2);
        }
    }
}
