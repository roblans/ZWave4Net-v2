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

        public async Task<NeighborUpdateStatus> RequestNeighborUpdate(Action<NeighborUpdateStatus> onProgress, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var writer = new PayloadWriter())
            {
                // get next callbackID (1..255) 
                var callbackID = MessageChannel.GetNextCallbackID();

                // write the ID of the node
                writer.WriteByte(NodeID);

                // write the callback
                writer.WriteByte(callbackID);

                // build the host message
                var message = new HostMessage(Function.RequestNodeNeighborUpdate, writer.GetPayload())
                {
                    Timeout = TimeSpan.FromSeconds(5),
                };

                // send request
                var requestNodeNeighborUpdate = await Channel.Send(message, (response) =>
                {
                    // response received, so open a reader
                    using (var reader = new PayloadReader(response.Payload))
                    {
                        // check if callback matches request 
                        if (reader.ReadByte() == callbackID)
                        {
                            // yes, so read status
                            var status = (NeighborUpdateStatus)reader.ReadByte();

                            // if callback delegate provided then invoke with progress 
                            onProgress?.Invoke(status);

                            // return true when final state reached (we're done)
                            return status == NeighborUpdateStatus.Done || status == NeighborUpdateStatus.Failed;
                        }
                    }
                    return false;
                }, cancellationToken);

                using (var reader = new PayloadReader(requestNodeNeighborUpdate.Payload))
                {
                    // skip the callback
                    reader.SkipBytes(1);

                    // return the status of the final response
                    return (NeighborUpdateStatus)reader.ReadByte();
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
