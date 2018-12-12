using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public class NodeUpdate<T> : IPayloadSerializable where T : IPayloadSerializable, new()
    {
        public NodeUpdateState State { get; private set; }
        public byte NodeID { get; private set; }
        public T Data { get; private set; }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            State = (NodeUpdateState)reader.ReadByte();
            NodeID = reader.ReadByte();

            var length = reader.ReadByte();
            if (length > 0)
            {
                Data = reader.ReadObject<T>();
            }
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
