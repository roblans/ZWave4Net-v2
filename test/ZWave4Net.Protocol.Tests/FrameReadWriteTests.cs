using System;
using System.Linq;
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


        [TestMethod]
        public async Task WriteReadDataFrame()
        {
            var input = new byte[] {1,12,0,4,0,15,6,49,5,4,34,0,3,239};
            var stream = new MockByteStream();

            stream.Write(input);
            stream.ResetPosition();
            var reader = new FrameReader(stream);
            var response = (DataFrame)(await reader.Read(CancellationToken.None));

            stream.ResetPosition();

            var request = new DataFrame(DataFrameType.REQ, response.Payload);
            var writer = new FrameWriter(stream);
            await writer.Write(request, CancellationToken.None);

            var output = stream.ToArray();

            Assert.IsTrue(input.SequenceEqual(output));
        }
    }
}
