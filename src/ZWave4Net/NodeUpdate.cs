using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public class NodeUpdate : IPayloadSerializable
    {
        public NodeUpdateState State { get; private set; }
        public byte NodeID { get; private set; }
        public NodeInfo Info { get; private set; }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            State = (NodeUpdateState)reader.ReadByte();
            NodeID = reader.ReadByte();

            var length = reader.ReadByte();
            if (length > 0 && State == NodeUpdateState.InfoReceived)
            {
                // push NodeID in the payload so NodeInfo has access to the node
                var payload = new Payload(new byte[] { NodeID }.Concat(reader.ReadBytes(reader.Length - reader.Position)));
                Info = payload.Deserialize<NodeInfo>();
            }
        }
        public override string ToString()
        {
            return $"{State} {NodeID} {Info}";
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
