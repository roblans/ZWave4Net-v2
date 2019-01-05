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
            : this(Convert.ToByte(@class) , Convert.ToByte(command), payload)
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

        public override string ToString()
        {
            return $"{ClassID}, {CommandID}, {Payload}";
        }

        protected virtual void Read(PayloadReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var length = reader.ReadByte();
            var classID = reader.ReadByte();
            var commandID = reader.ReadByte();
            var data = default(byte[]);

            // SDS12657-12-Z-Wave-Command-Class-Specification-A-M.pdf | 4.41.1 CRC-16 Encapsulated Command
            // The CRC-16 Encapsulation Command is used to encapsulate a command with an additional checksum to ensure integrity of the payload
            if (classID == (byte)CommandClass.Crc16Encap && commandID == 1)
            {
                ClassID = reader.ReadByte();
                CommandID = reader.ReadByte();
                data = reader.ReadBytes(length - 6);

                var actualChecksum = reader.ReadInt16();
                var expectedChecksum = new byte[] { classID, commandID, ClassID, CommandID }.Concat(data).CalculateCrc16Checksum();

                if (actualChecksum != expectedChecksum)
                    throw new Crc16ChecksumException("CRC-16 encapsulated command checksum failure");
            }
            else
            {
                ClassID = classID;
                CommandID = commandID;
                data = reader.ReadBytes(length - 2);
            }

            Payload = new Payload(data);
        }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Read(reader);
        }

        protected virtual void Write(PayloadWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteByte((byte)(2 + Payload.Length));
            writer.WriteByte(ClassID);
            writer.WriteByte(CommandID);
            writer.WriteObject(Payload);
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            Write(writer);
        }
    }
}
