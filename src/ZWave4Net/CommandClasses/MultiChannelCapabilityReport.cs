using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ZWave.CommandClasses
{
    public class MultiChannelCapabilityReport : Report
    {
        public byte EndpointID { get; private set; }
        public GenericType GenericType { get; private set; }
        public SpecificType SpecificType { get; private set; }
        public CommandClass[] SupportedCommandClasses { get; private set; } = new CommandClass[0];

        protected override void Read(PayloadReader reader)
        {
            EndpointID = (byte)(reader.ReadByte() & 0x7F);
            GenericType = (GenericType)reader.ReadByte();
            SpecificType = reader.ReadSpecificType(GenericType);
            SupportedCommandClasses = reader.ReadBytes(reader.Length - reader.Position).Select(element => (CommandClass)element).ToArray();
        }

        public override string ToString()
        {
            return $"EndpointID: {EndpointID}, GenericType = {GenericType}, SpecificType = {SpecificType}, CommandClasses = {string.Join(", ", SupportedCommandClasses)}";
        }
    }
}
