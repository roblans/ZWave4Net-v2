using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public abstract class Report : IPayloadSerializable
    {
        public Address Source { get; private set; }

        protected abstract void Read(PayloadReader reader);

        public override string ToString()
        {
            return $"{GetType().Name}: Source: {Source}";
        }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            var nodeID = reader.ReadByte();
            var endpointID = reader.ReadByte();
            Source = new Address(nodeID, endpointID);

            Read(reader);
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
