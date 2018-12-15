using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    public class EncapsulatedCommand : ICommand
    {
        public byte ClassID { get; private set; }
        public byte CommandID { get; private set; }
        public byte SourceEndpointID { get; private set; }
        public byte TargetEndpointID { get; private set; }
        public byte[] Payload { get; private set; }
        public ICommand Command { get; private set; }

        public EncapsulatedCommand()
        {
            ClassID = Convert.ToByte(CommandClass.MultiChannel);
            CommandID = 0x0D;
        }

        public EncapsulatedCommand(byte sourceEndpointID, byte targetEndpointID, ICommand command)
            : this()
        {
            SourceEndpointID = sourceEndpointID;
            TargetEndpointID = targetEndpointID;
            Payload = command.Serialize().ToArray().Skip(1).ToArray();
            Command = command;
        }

        public override string ToString()
        {
            return $"{SourceEndpointID}, {TargetEndpointID}, {BitConverter.ToString(Payload)}";
        }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            var length = reader.ReadByte();
            ClassID = reader.ReadByte();
            CommandID = reader.ReadByte();
            SourceEndpointID = reader.ReadByte();
            TargetEndpointID = reader.ReadByte();
            Payload = reader.ReadBytes(length - 4);
            Command = new Command(Payload[0], Payload[1], Payload.Skip(2).ToArray());
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            writer.WriteByte((byte)(4 + Payload.Length));
            writer.WriteByte(ClassID);
            writer.WriteByte(CommandID);
            writer.WriteByte(SourceEndpointID);
            writer.WriteByte(TargetEndpointID);
            writer.WriteBytes(Payload);
        }
    }
}
