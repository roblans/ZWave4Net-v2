using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    internal class Command : IPayloadSerializable
    {
        public byte ClassID { get; protected set; }
        public byte CommandID { get; protected set; }
        public Payload Payload { get; protected set; }

        public Command()
        {
        }

        public Command(CommandClass @class, Enum command, params byte[] payload)
            : this(Convert.ToByte(@class), Convert.ToByte(command), payload)
        {
        }

        public Command(CommandClass @class, Enum command, IEnumerable<byte> payload)
            : this(Convert.ToByte(@class), Convert.ToByte(command), payload)
        {
        }

        public Command(byte classID, byte commandID, params byte[] payload)
        {
            ClassID = classID;
            CommandID = commandID;
            Payload = payload != null ? new Payload(payload) : Payload.Empty;
        }

        public Command(byte classID, byte commandID, Payload payload)
        {
            ClassID = classID;
            CommandID = commandID;
            Payload = payload;
        }

        public Command(byte classID, byte commandID, IEnumerable<byte> payload)
        {
            ClassID = classID;
            CommandID = commandID;
            Payload = payload != null ? new Payload(payload) : Payload.Empty;
        }

        public static IEnumerable<Command> Decapsulate(Command command)
        {
            yield return command;
            while (command is EncapsulatedCommand encapsulatedCommand)
            {
                command = encapsulatedCommand.Decapsulate();
                yield return command;
            }
        }

        public override string ToString()
        {
            return $"{ClassID}, {CommandID}, {Payload}";
        }

        protected virtual void Read(PayloadReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            ClassID = reader.ReadByte();
            CommandID = reader.ReadByte();
            Payload = new Payload(reader.ReadBytes(reader.Length - reader.Position));
        }

        protected virtual void Write(PayloadWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteByte(ClassID);
            writer.WriteByte(CommandID);
            writer.WriteObject(Payload);
        }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            Read(reader);
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
           Write(writer);
        }
    }
}
