using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZWave.Channel.Protocol;
using ZWave.Channel.Protocol.Frames;

namespace ZWave.Tests
{
    [TestClass]
    public class FrameReadWriteTests
    {
        [TestMethod]
        public async Task WriteReadAckFrame()
        {
            var stream = new MockDuplexStream();

            var writer = new FrameWriter(stream);
            var request = Frame.ACK;
            await writer.Write(request, CancellationToken.None);

            var data = stream.Output.ToArray();
            await stream.Input.WriteAsync(data, 0, data.Length);
            stream.Input.Position = 0;

            var reader = new FrameReader(stream);
            var response = await reader.Read(CancellationToken.None);

            Assert.AreEqual(request, response);
        }


        [TestMethod]
        public async Task WriteReadDataFrame()
        {
            var input = new byte[] { 1, 12, 0, 4, 0, 15, 6, 49, 5, 4, 34, 0, 3, 239 };
            var stream = new MockDuplexStream();

            stream.Input.Write(input, 0, input.Length);
            stream.Input.Position = 0;
            var reader = new FrameReader(stream);
            var response = (DataFrame)(await reader.Read(CancellationToken.None));

            var request = new DataFrame(DataFrameType.REQ, response.Payload);
            var writer = new FrameWriter(stream);
            await writer.Write(request, CancellationToken.None);

            var output = stream.Output.ToArray();

            Assert.IsTrue(input.SequenceEqual(output));
        }
    }
}
