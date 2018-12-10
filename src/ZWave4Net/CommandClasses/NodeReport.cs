using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public abstract class NodeReport : IPayloadSerializable
    {
        public byte NodeID { get; private set; }

        protected abstract void Read(PayloadReader reader);

        public override string ToString()
        {
            return $"{GetType().Name}: Node: {NodeID:D3}";
        }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            NodeID = reader.ReadByte();
            Read(reader);
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
