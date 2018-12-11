using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public class NodeUpdate : IPayloadSerializable
    {
        public NodeUpdateState State { get; private set; }
        public byte NodeID { get; private set; }
        public PayloadBytes Payload { get; private set; }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            State = (NodeUpdateState)reader.ReadByte();
            NodeID = reader.ReadByte();
            Payload = new PayloadBytes(reader.ReadBytes(reader.Length - reader.Position));
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
