using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void Crc16CommandDeserialize()
        {
            var payload = new Payload(new byte[] { 6, 0x56, 0x01, 0x20, 0x02, 0x4D, 0x26 });
            var command = payload.Deserialize<Command>();
        }
    }
}
