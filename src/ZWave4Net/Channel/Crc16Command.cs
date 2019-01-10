using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ZWave4Net.CommandClasses;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    // <summary>
    // SDS12657-12-Z-Wave-Command-Class-Specification-A-M.pdf | 4.41.1 CRC-16 Encapsulated Command
    // The CRC-16 Encapsulation Command is used to encapsulate a command with an additional checksum to ensure integrity of the payload
    // </summary>
    internal class Crc16Command : Command, IEncapsulatedCommand
    {
        public const byte EncapCommandID = 1;

        private Crc16Command(Payload payload)
            : base(CommandClass.Crc16Encap, EncapCommandID, payload)
        {
        }

        public Crc16Command()
        {
        }
        
        public static Crc16Command Encapsulate(Command command)
        {
            var payload = command.Serialize();
            return new Crc16Command(payload);
        }

        public Command Decapsulate()
        {
            return Command.Parse(Payload);
        }

        protected override void Read(PayloadReader reader)
        {
            CommandClass = (CommandClass)reader.ReadByte();
            CommandID = reader.ReadByte();
            Payload = new Payload(reader.ReadBytes(reader.Length - reader.Position - 2));

            var actualChecksum = reader.ReadInt16();
            var expectedChecksum = new byte[] { (byte)CommandClass, CommandID }.Concat(Payload.ToArray()).CalculateCrc16Checksum();

            if (actualChecksum != expectedChecksum)
                throw new ChecksumException("CRC-16 encapsulated command checksum failure");
        }

        protected override void Write(PayloadWriter writer)
        {
            writer.WriteByte((byte)CommandClass);
            writer.WriteByte(CommandID);
            writer.WriteObject(Payload);

            var checksum = new byte[] { (byte)CommandClass, CommandID }.Concat(Payload.ToArray()).CalculateCrc16Checksum();
            writer.WriteInt16(checksum);
        }
    }
}
