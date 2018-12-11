using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.CommandClasses;

namespace ZWave4Net
{
    public class NodeInfo : IPayloadSerializable
    {
        public BasicType BasicType { get; private set; }
        public GenericType GenericType { get; private set; }
        public SpecificType SpecificType { get; private set; }
        public CommandClass[] SupportedCommandClasses { get; private set; } = new CommandClass[0];

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            var unknown1 = reader.ReadByte();
            var unknown2 = reader.ReadByte();
            var length = reader.ReadByte();

            if (reader.Position < reader.Length)
            {
                BasicType = (BasicType)reader.ReadByte();
            }

            if (reader.Position < reader.Length)
            {
                GenericType = (GenericType)reader.ReadByte();
            }

            if (reader.Position < reader.Length)
            {
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

            if (reader.Position < reader.Length)
            {
                SupportedCommandClasses = reader
                    .ReadBytes(reader.Length - reader.Position)
                    .TakeWhile(x => x != 0xEF)
                    .Select(x => (CommandClass)x)
                    .ToArray();
            }
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
