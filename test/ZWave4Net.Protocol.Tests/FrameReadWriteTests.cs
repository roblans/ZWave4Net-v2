using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Protocol.Tests
{
    [TestClass]
    public class FrameReadWriteTests
    {
        [TestMethod]
        public async Task WriteReadAckFrame()
        {
            var stream = new MockByteStream();

            var writer = new FrameWriter(stream);
            var request = Frame.ACK;
            await writer.Write(request, CancellationToken.None);

            stream.ResetPosition();

            var reader = new FrameReader(stream);
            var response = await reader.Read(CancellationToken.None);

            Assert.AreEqual(request, response);
        }

        //[TestMethod]
        //public async Task WriteReadDataFrame()
        //{
        //    var stream = new MockByteStream();

        //    var writer = new FrameWriter(stream);
        //    var parameters = new byte[] { 0, 1, 2, 3, 4, 5 };
        //    var request = new RequestDataFrame(ControllerFunction.ClockGet);
        //    await writer.Write(request, CancellationToken.None);

        //    stream.ResetPosition();

        //    var reader = new FrameReader(stream);
        //    var response = await reader.Read(CancellationToken.None);

        //    Assert.AreEqual(request, response);
        //}
    }
}
