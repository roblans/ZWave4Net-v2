using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    internal class Command
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

        public static Command Deserialize(Payload payload)
        {
            if (payload.Length < 3)
                throw new ArgumentOutOfRangeException(nameof(payload), "payload must have at least 3 bytes");

            var classID = payload[1];

            var command = default(Command);
            switch (classID)
            {
                case (byte)CommandClass.Crc16Encap:
                    command = new Crc16EndcapCommand();
                    break;
                case (byte)CommandClass.MultiChannel:
                    command = new MultiChannelCommand();
                    break;
                default:
                    command = new Command();
                    break;
            }
            using (var reader = new PayloadReader(payload.ToArray().Skip(1)))
            {
                command.Read(reader);
            }
            return command;
        }

        public static Command Decapsulate(Command command)
        {
            while(command is IEncapsulatedCommand encapsulatedCommand)
            {
                command = encapsulatedCommand.Decapsulate();
            }
            return command;
        }

        public Payload Serialize()
        {
            using (var writer = new PayloadWriter())
            {
                Write(writer);

                var payload = writer.ToByteArray();
                return new Payload(new[] { (byte)payload.Length }.Concat(payload));
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
    }
}
