using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public class NodeProtocolInfo : IPayloadSerializable
    {
        public byte Capability { get; private set; }
        public byte Reserved { get; private set; }
        public BasicType BasicType { get; private set; }
        public GenericType GenericType { get; private set; }
        public SpecificType SpecificType { get; private set; }
        public Security Security { get; private set; }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            Capability = reader.ReadByte();
            Security = (Security)reader.ReadByte();
            Reserved = reader.ReadByte();
            BasicType = (BasicType)reader.ReadByte();
            GenericType = (GenericType)reader.ReadByte();

            var specificType = reader.ReadByte();
            if (specificType == 0)
            {
                SpecificType = SpecificType.NotUsed;
            }
            else
            {
                SpecificType = (SpecificType)((int)GenericType << 16 | specificType);
            }

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
            return $"BasicType = {BasicType}, GenericType = {GenericType}, SpecificType = {SpecificType}, Listening = {IsListening}, Version = {Version}, Security = [{Security}], Routing = {Routing}, MaxBaudrate = {MaxBaudrate}";
        }
    }
}
