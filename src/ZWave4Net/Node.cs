using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net
{
    public class Node : Endpoint, IEquatable<Node>
    {
        public byte NodeID { get; private set; }
        public EndpointCollection Endpoints { get; private set; }

        public Node(byte nodeID, ZWaveController controller) : base(controller)
        {
            NodeID = nodeID;
            Endpoints = new EndpointCollection(this);
        }

        public async Task<NodeProtocolInfo> GetProtocolInfo(CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new RequestCommand<Payload>(Function.GetNodeProtocolInfo, new Payload(NodeID));
            return await Channel.Send<NodeProtocolInfo>(command, cancellationToken);
        }

        public async Task<Node[]> GetNeighbours(CancellationToken cancellationToken = default(CancellationToken))
        {
            var results = new List<Node>();

            var command = new RequestCommand(Function.GetRoutingTableLine, new Payload(NodeID));

            // send request
            var response = await Channel.Send(command, cancellationToken);

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


        public async Task<NeighborUpdateStatus> RequestNeighborUpdate(Action<NeighborUpdateStatus> onProgress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new RequestCommand(Function.RequestNodeNeighborUpdate, new Payload(NodeID), useCallbackID: true);

            var requestNodeNeighborUpdate = await Channel.Send(command, (progress) =>
            {
                var status = (NeighborUpdateStatus)progress.ToArray()[0];

                onProgress?.Invoke(status);

                return status == NeighborUpdateStatus.Done || status == NeighborUpdateStatus.Failed;
            },
            cancellationToken);

            // return the status of the final response
            return (NeighborUpdateStatus)requestNodeNeighborUpdate.ToArray()[0];
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Node);
        }

        public bool Equals(Node other)
        {
            return other != null &&
                   NodeID == other.NodeID;
        }

        public override int GetHashCode()
        {
            return -1960697928 + NodeID.GetHashCode();
        }

        public override string ToString()
        {
            return $"{NodeID:D3}";
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
