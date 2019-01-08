using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ZWave4Net;
using ZWave4Net.Channel;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Tests
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
                6,
                (byte)CommandClass.Crc16Encap,
                0x01,
                (byte)CommandClass.Basic,
                0x02,
                0x4D,
                0x26 });
            var command = Command.Decapsulate(Command.Deserialize(payload));
            Assert.AreEqual(command.ClassID, (byte)CommandClass.Basic);
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

            var command = Crc16EndcapCommand.Encapsulate(new Command((byte)CommandClass.Basic, 0x02));
            var payload = command.Serialize().ToArray();

            Assert.AreEqual(payload[0], 6);
            Assert.AreEqual(payload[1], (byte)CommandClass.Crc16Encap);
            Assert.AreEqual(payload[2], 0x01);
            Assert.AreEqual(payload[3], (byte)CommandClass.Basic);
            Assert.AreEqual(payload[4], 0x02);
            Assert.AreEqual(payload[5], 0x4D);
            Assert.AreEqual(payload[6], 0x26);
        }

        [TestMethod]
        public void MultiEncapDecap()
        {
            var multiChannelCommand = MultiChannelCommand.Encapsulate(0, 1, Crc16EndcapCommand.Encapsulate(new Command((byte)CommandClass.Basic, 0x02)));

            Assert.AreEqual(multiChannelCommand.SourceEndpointID, 0);
            Assert.AreEqual(multiChannelCommand.TargetEndpointID, 1);
            Assert.AreEqual(multiChannelCommand.ClassID, (byte)CommandClass.MultiChannel);
            Assert.AreEqual(multiChannelCommand.CommandID, 0x0D);

            var crc16EndcapCommand = ((IEncapsulatedCommand)multiChannelCommand).Decapsulate();
            Assert.AreEqual(crc16EndcapCommand.ClassID, (byte)CommandClass.Crc16Encap);
            Assert.AreEqual(crc16EndcapCommand.CommandID, 0x01);

            var basicCommand = ((IEncapsulatedCommand)crc16EndcapCommand).Decapsulate();
            Assert.AreEqual(basicCommand.ClassID, (byte)CommandClass.Basic);
            Assert.AreEqual(basicCommand.CommandID, 0x02);

            basicCommand = Command.Decapsulate(multiChannelCommand);
            Assert.AreEqual(basicCommand.ClassID, (byte)CommandClass.Basic);
            Assert.AreEqual(basicCommand.CommandID, 0x02);
        }
    }
}
