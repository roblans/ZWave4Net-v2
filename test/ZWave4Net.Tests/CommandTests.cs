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
            var command = payload.Deserialize<Command>();
            Assert.AreEqual(command.ClassID, (byte)CommandClass.Basic);
            Assert.AreEqual(command.CommandID, 0x02);
            Assert.IsTrue(command.Crc16Checksum);
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

            var command = new Command((byte)CommandClass.Basic, 0x02)
            {
                Crc16Checksum = true,
            };
            var payload = command.Serialize().ToArray();

            Assert.AreEqual(payload[0], 6);
            Assert.AreEqual(payload[1], (byte)CommandClass.Crc16Encap);
            Assert.AreEqual(payload[2], 0x01);
            Assert.AreEqual(payload[3], (byte)CommandClass.Basic);
            Assert.AreEqual(payload[4], 0x02);
            Assert.AreEqual(payload[5], 0x4D);
            Assert.AreEqual(payload[6], 0x26);
        }
    }
}
