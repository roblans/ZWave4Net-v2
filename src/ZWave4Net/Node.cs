using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net
{
    public class Node : IEquatable<Node>
    {
        public readonly byte NodeID;
        public readonly ZWaveController Controller;

        public Node(byte nodeID, ZWaveController controller)
        {
            Controller = controller;
            NodeID = nodeID;
        }

        private MessageChannel Channel
        {
            get { return Controller.Channel; }
        }

        public async Task<NodeProtocolInfo> GetProtocolInfo(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var writer = new PayloadWriter())
            {
                writer.WriteByte(NodeID);

                var message = new HostMessage(Function.GetNodeProtocolInfo, writer.GetPayload());
                var response = await Channel.Send(message, cancellationToken);

                using (var reader = new PayloadReader(response.Payload))
                {
                    return reader.ReadObject<NodeProtocolInfo>();
                }
            } 
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
