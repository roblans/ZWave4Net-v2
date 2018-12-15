using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    public interface ICommand : IPayloadSerializable
    {
        byte ClassID { get; }
        byte CommandID { get; }
        byte[] Payload { get; }
    }

    public class Command : ICommand
    {
        public byte ClassID { get; private set; }
        public byte CommandID { get; private set; }
        public byte[] Payload { get; private set; }

        public Command()
        {
        }

        public Command(CommandClass @class, Enum command, params byte[] payload)
            : this(Convert.ToByte(@class) , Convert.ToByte(command), payload)
        {
        }

        public Command(byte classID, byte commandID, params byte[] payload)
        {
            ClassID = classID;
            CommandID = commandID;
            Payload = payload;
        }

        public override string ToString()
        {
            return $"{ClassID}, {CommandID}, {BitConverter.ToString(Payload)}";
        }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            var length = reader.ReadByte();
            ClassID = reader.ReadByte();
            CommandID = reader.ReadByte();
            Payload = reader.ReadBytes(length - 2);
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            writer.WriteByte((byte)(2 + Payload.Length));
            writer.WriteByte(ClassID);
            writer.WriteByte(CommandID);
            writer.WriteBytes(Payload);
        }
    }
}
