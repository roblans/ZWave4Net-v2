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
            if (payload.Length < 2)
                throw new ArgumentOutOfRangeException(nameof(payload), "payload must have at least 2 bytes");

            var command = default(Command);

            var classID = payload[0];
            var commandID = payload[1];
            
            switch (classID)
            {
                case Crc16EndcapCommand.EncapClassID when Crc16EndcapCommand.EncapCommandID == commandID:
                    command = new Crc16EndcapCommand();
                    break;
                case MultiChannelEndcapCommand.EncapClassID when MultiChannelEndcapCommand.EncapCommandID == commandID:
                    command = new MultiChannelEndcapCommand();
                    break;
                default:
                    command = new Command();
                    break;
            }
            using (var reader = new PayloadReader(payload))
            {
                command.Read(reader);
            }
            return command;
        }

        public static IEnumerable<Command> Decapsulate(Command command)
        {
            yield return command;
            while (command is IEncapsulatedCommand encapsulatedCommand)
            {
                command = encapsulatedCommand.Decapsulate();
                yield return command;
            }
        }

        public Payload Serialize()
        {
            using (var writer = new PayloadWriter())
            {
                Write(writer);

                return writer.ToPayload();
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
