﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    // <summary>
    // SDS12657-12-Z-Wave-Command-Class-Specification-A-M.pdf | 4.41.1 CRC-16 Encapsulated Command
    // The CRC-16 Encapsulation Command is used to encapsulate a command with an additional checksum to ensure integrity of the payload
    // </summary>
    internal class Crc16EndcapCommand : Command, IEncapsulatedCommand
    {
        const byte Crc16EncapCommandID = 1;

        public Crc16EndcapCommand()
        {
        }

        private Crc16EndcapCommand(Payload payload)
            : base(Convert.ToByte(CommandClass.Crc16Encap), Crc16EncapCommandID, payload)
        {
            Payload = payload;
        }

        public static Crc16EndcapCommand Encapsulate(Command command)
        {
            var payload = new Payload(command.Serialize().ToArray().Skip(1).ToArray());
            return new Crc16EndcapCommand(payload);
        }

        Command IEncapsulatedCommand.Decapsulate()
        {
            var payload = new Payload(new[] { (byte)(Payload.Length + 2) }.Concat(Payload.ToArray()));
            return Deserialize(payload);
        }

        protected override void Read(PayloadReader reader)
        {
            ClassID = reader.ReadByte();
            CommandID = reader.ReadByte();
            Payload = new Payload(reader.ReadBytes(reader.Length - reader.Position - 2));

            var actualChecksum = reader.ReadInt16();
            var expectedChecksum = new byte[] { ClassID, CommandID }.Concat(Payload.ToArray()).CalculateCrc16Checksum();

            if (actualChecksum != expectedChecksum)
                throw new Crc16ChecksumException("CRC-16 encapsulated command checksum failure");
        }

        protected override void Write(PayloadWriter writer)
        {
            writer.WriteByte(ClassID);
            writer.WriteByte(CommandID);
            writer.WriteObject(Payload);

            var checksum = new byte[] { ClassID, CommandID }.Concat(Payload.ToArray()).CalculateCrc16Checksum();
            writer.WriteInt16(checksum);
        }
    }
}