using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    public class Command : IPayloadSerializable
    {
        public byte ClassID { get; protected set; }
        public byte CommandID { get; protected set; }
        public byte[] Payload { get; protected set; }

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

        protected virtual void Read(PayloadReader reader)
        {
            var length = reader.ReadByte();
            ClassID = reader.ReadByte();
            CommandID = reader.ReadByte();
            Payload = reader.ReadBytes(length - 2);
        }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            Read(reader);
        }

        protected virtual void Write(PayloadWriter writer)
        {
            writer.WriteByte((byte)(2 + Payload.Length));
            writer.WriteByte(ClassID);
            writer.WriteByte(CommandID);
            writer.WriteBytes(Payload);
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            Write(writer);
        }
    }
}
