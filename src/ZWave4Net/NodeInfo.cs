using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.CommandClasses;

namespace ZWave4Net
{
    public class NodeInfo : IPayloadSerializable
    {
        public byte NodeID { get; private set; }
        public NodeType NodeType { get; private set; }
        public CommandClass[] SupportedCommandClasses { get; private set; } = new CommandClass[0];

        public override string ToString()
        {
            return $"Node: {NodeID}, Type = {NodeType}, CommandClasses = {string.Join(", ", SupportedCommandClasses)}";
        }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            NodeID = reader.ReadByte();
            var basicType = (BasicType)reader.ReadByte();
            var genericType = (GenericType)reader.ReadByte();
            var specificType = reader.ReadSpecificType(genericType);

            NodeType = new NodeType(basicType, genericType, specificType);

            SupportedCommandClasses = reader
                .ReadBytes(reader.Length - reader.Position)
                .TakeWhile(x => x != 0xEF)
                .Select(x => (CommandClass)x)
                .OrderBy(element => element.ToString())
                .ToArray();
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
