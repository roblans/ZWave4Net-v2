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
        public async Task ReadEventDataFrame()
        {
            var stream = new MockByteStream();
            var data = new byte[]
            {
                (byte)FrameHeader.SOF,
                0x0A, // length 
                (byte)DataFrameType.REQ,
                (byte)ControllerFunction.ApplicationCommandHandler,
                0x04, // TypeSingle
                0x01, // NodeID
                0x31, // SensorMultiLevel
                0x05, // Report
                0x04, 0x22, 0x00, 0x00, // Payload
                0x00 // checksum
            };

            var checksum = data.Skip(1).Take(data.Length - 2).CalculateChecksum();
            data[data.Length - 1] = checksum;

            stream.Write(data);

            stream.ResetPosition();

            var reader = new FrameReader(stream);
            var frame = (EventDataFrame)(await reader.Read(CancellationToken.None));

            Assert.AreEqual(frame.Header, FrameHeader.SOF);
            Assert.AreEqual(frame.Function, ControllerFunction.ApplicationCommandHandler);
            Assert.IsTrue(frame.Payload.SequenceEqual(data.Skip(4).Take(8)));
        }

        [TestMethod]
        public async Task WriteRequestDataFrame()
        {
            var payload = new byte[]
            {
                0x04, // TypeSingle
                0x01, // NodeID
                0x31, // SensorMultiLevel
                0x05, // Report
                0x04, 0x22, 0x00, 0x00, // Payload
            };

            var request = new RequestDataFrame(ControllerFunction.ApplicationCommandHandler, payload);
            var stream = new MockByteStream();

            var writer = new FrameWriter(stream);
            await writer.Write(request, CancellationToken.None);

            var data = stream.ToArray();

            Assert.AreEqual(data[0], (byte)FrameHeader.SOF);
            Assert.AreEqual(data[2], (byte)DataFrameType.REQ);
            Assert.AreEqual(data[3], (byte)ControllerFunction.ApplicationCommandHandler);
            Assert.IsTrue(request.Payload.SequenceEqual(payload));
        }

        [TestMethod]
        public async Task ReadWriteDataFrame()
        {
            var payload = new byte[]
            {
                0x04, // TypeSingle
                0x01, // NodeID
                0x31, // SensorMultiLevel
                0x05, // Report
                0x04, 0x22, 0x00, 0x00, // Payload
            };

            var request = new RequestDataFrame(ControllerFunction.ApplicationCommandHandler, payload);
            var stream = new MockByteStream();

            var writer = new FrameWriter(stream);
            await writer.Write(request, CancellationToken.None);

            var data = stream.ToArray();
            // patch direction
            data[2] = (byte)DataFrameType.RES;
            // patch checksum
            data[data.Length - 1] = (byte)(data[data.Length - 1] + 1);

            stream.ResetPosition();
            stream.Write(data);

            stream.ResetPosition();
            var reader = new FrameReader(stream);
            var response = (ResponseDataFrame)(await reader.Read(CancellationToken.None));

            Assert.AreEqual(request.Header, response.Header);
            Assert.AreEqual(request.Function, response.Function);
            Assert.IsTrue(request.Payload.SequenceEqual(response.Payload));
        }
    }
}
