using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ZWave;
using ZWave.Channel;
using ZWave.CommandClasses;

namespace ZWave.Tests
{
    /// <summary>
    /// Summary description for CommandTests
    /// </summary>
    [TestClass]
    public class CommandTests
    {
        [TestMethod]
        public void Crc16EncapCommandDeserialize()
        {
            // SDS12657-12-Z-Wave-Command-Class-Specification-A-M.pdf | 4.41.1 CRC-16 Encapsulated Command
            // 4.41.2 Example 
            // CRC - 16 Encapsulation Command Class = 0x56 
            // CRC - 16 Encapsulation Command = 0x01 
            // Basic Command Class = 0x20 
            // Basic Get Command = 0x02 
            // CRC1 = 0x4D 
            // CRC2 = 0x26
            var payload = new Payload(new byte[] 
            {
                (byte)CommandClass.Crc16Encap,
                0x01,
                (byte)CommandClass.Basic,
                0x02,
                0x4D,
                0x26 });
            var command = Encapsulation.Flatten(Command.Parse(payload)).Last();
            Assert.AreEqual(command.CommandClass, CommandClass.Basic);
            Assert.AreEqual(command.CommandID, 0x02);
        }

        [TestMethod]
        public void Crc16EncapCommandSerialize()
        {
            // SDS12657-12-Z-Wave-Command-Class-Specification-A-M.pdf | 4.41.1 CRC-16 Encapsulated Command
            // 4.41.2 Example 
            // CRC - 16 Encapsulation Command Class = 0x56 
            // CRC - 16 Encapsulation Command = 0x01 
            // Basic Command Class = 0x20 
            // Basic Get Command = 0x02 
            // CRC1 = 0x4D 
            // CRC2 = 0x26

            var command = Crc16Command.Encapsulate(new Command(CommandClass.Basic, 0x02));
            var payload = command.Serialize().ToArray();

            Assert.AreEqual(payload[0], (byte)CommandClass.Crc16Encap);
            Assert.AreEqual(payload[1], 0x01);
            Assert.AreEqual(payload[2], (byte)CommandClass.Basic);
            Assert.AreEqual(payload[3], 0x02);
            Assert.AreEqual(payload[4], 0x4D);
            Assert.AreEqual(payload[5], 0x26);
        }

        [TestMethod]
        public void MultiEncapDecap()
        {
            var crc16Command = Crc16Command.Encapsulate(MultiChannelCommand.Encapsulate(0, 1, new Command(CommandClass.Basic, 0x02)));

            Assert.AreEqual(crc16Command.CommandClass, CommandClass.Crc16Encap);
            Assert.AreEqual(crc16Command.CommandID, 0x01);

            var multiChannelCommand = (MultiChannelCommand)crc16Command.Decapsulate();
            Assert.AreEqual(multiChannelCommand.SourceEndpointID, 0);
            Assert.AreEqual(multiChannelCommand.TargetEndpointID, 1);
            Assert.AreEqual(multiChannelCommand.CommandClass, CommandClass.MultiChannel);
            Assert.AreEqual(multiChannelCommand.CommandID, 0x0D);

            var basicCommand = multiChannelCommand.Decapsulate();
            Assert.AreEqual(basicCommand.CommandClass, CommandClass.Basic);
            Assert.AreEqual(basicCommand.CommandID, 0x02);

            var commands = Encapsulation.Flatten(crc16Command).ToArray();
            Assert.AreEqual(commands.Length, 3);

            crc16Command = (Crc16Command)commands[0];
            Assert.AreEqual(crc16Command.CommandClass, CommandClass.Crc16Encap);
            Assert.AreEqual(crc16Command.CommandID, 0x01);

            multiChannelCommand = (MultiChannelCommand)commands[1];
            Assert.AreEqual(multiChannelCommand.SourceEndpointID, 0);
            Assert.AreEqual(multiChannelCommand.TargetEndpointID, 1);
            Assert.AreEqual(multiChannelCommand.CommandClass, CommandClass.MultiChannel);
            Assert.AreEqual(multiChannelCommand.CommandID, 0x0D);

            basicCommand = commands[2];
            Assert.AreEqual(basicCommand.CommandClass, CommandClass.Basic);
            Assert.AreEqual(basicCommand.CommandID, 0x02);
        }
    }
}
