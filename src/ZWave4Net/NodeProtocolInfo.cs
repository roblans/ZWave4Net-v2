using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public class NodeProtocolInfo : IPayloadSerializable
    {
        public byte Capability { get; private set; }
        public byte Reserved { get; private set; }
        public NodeType NodeType { get; private set; }
        public Security Security { get; private set; }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Capability = reader.ReadByte();
            Security = (Security)reader.ReadByte();
            Reserved = reader.ReadByte();

            var basicType = (BasicType)reader.ReadByte();
            var genericType = (GenericType)reader.ReadByte();
            var specificType = reader.ReadSpecificType(genericType);
            NodeType = new NodeType(basicType, genericType, specificType);
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }

        public bool Routing
        {
            get { return (Capability & 0x40) != 0; }
        }

        public bool IsListening
        {
            get { return (Capability & 0x80) != 0; }
        }

        public byte Version
        {
            get { return (byte)((Capability & 0x07) + 1); }
        }

        public int MaxBaudrate
        {
            get { return ((Capability & 0x38) == 0x10) ? 40000 : 9600; }
        }

        public override string ToString()
        {
            return $"NodeType = {NodeType}, Listening = {IsListening}, Version = {Version}, Security = [{Security}], Routing = {Routing}, MaxBaudrate = {MaxBaudrate}";
        }
    }
}
