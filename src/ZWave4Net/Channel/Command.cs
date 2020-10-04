using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave.Channel.Protocol;
using ZWave.CommandClasses;

namespace ZWave.Channel
{
    internal class Command : IPayloadSerializable
    {
        public CommandClass CommandClass { get; protected set; }
        public byte CommandID { get; protected set; }
        public Payload Payload { get; protected set; }

        public Command()
        {
        }

        public Command(CommandClass commandClass, Enum command, params byte[] payload)
            : this(commandClass, Convert.ToByte(command), payload)
        {
        }

        public Command(CommandClass commandClass, Enum command, IEnumerable<byte> payload)
            : this(commandClass, Convert.ToByte(command), payload)
        {
        }

        public Command(CommandClass classID, byte commandID, params byte[] payload)
        {
            CommandClass = classID;
            CommandID = commandID;
            Payload = payload != null ? new Payload(payload) : Payload.Empty;
        }

        public Command(CommandClass classID, byte commandID, Payload payload)
        {
            CommandClass = classID;
            CommandID = commandID;
            Payload = payload;
        }

        public Command(CommandClass classID, byte commandID, IEnumerable<byte> payload)
        {
            CommandClass = classID;
            CommandID = commandID;
            Payload = payload != null ? new Payload(payload) : Payload.Empty;
        }

        public static Command Parse(Payload payload)
        {
            if (payload.Length < 2)
                throw new ArgumentOutOfRangeException(nameof(payload), "payload must have at least 2 bytes");

            var commandClass = (CommandClass)payload[0];
            var commandID = payload[1];

            using (var reader = new PayloadReader(payload))
            {
                switch (commandClass)
                {
                    case CommandClass.Crc16Encap when Crc16Command.EncapCommandID == commandID:
                        return reader.ReadObject<Crc16Command>();
                    case CommandClass.MultiChannel when MultiChannelCommand.EncapCommandID == commandID:
                        return reader.ReadObject<MultiChannelCommand>();
                    default:
                        return reader.ReadObject<Command>();
                }
            }
        }

        public override string ToString()
        {
            return $"{CommandClass}, {CommandID}, {Payload}";
        }

        protected virtual void Read(PayloadReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            CommandClass = (CommandClass)reader.ReadByte();
            CommandID = reader.ReadByte();
            Payload = new Payload(reader.ReadBytes(reader.Length - reader.Position));
        }

        protected virtual void Write(PayloadWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteByte((byte)CommandClass);
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
