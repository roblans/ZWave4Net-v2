using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.CommandClasses;

namespace ZWave4Net
{
    /// <summary>
    /// Represents a Node Information Frame
    /// </summary>
    public class NodeInfo : IPayloadSerializable
    {
        /// <summary>
        /// The NodeID of the node
        /// </summary>
        public byte NodeID { get; private set; }

        /// <summary>
        /// The type of the node
        /// </summary>
        public NodeType NodeType { get; private set; }

        /// <summary>
        /// Command Classes implemented by the node
        /// </summary>
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
