using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.Channel.Protocol
{
    public class FrameReader
    {
        public readonly IByteStream Stream;

        public FrameReader(IByteStream stream)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public async Task<Frame> Read(CancellationToken cancelation)
        {
            while (true)
            {
                var header = await Stream.ReadHeader(cancelation);
                switch(header)
                {
                    case FrameHeader.ACK:
                        return Frame.ACK;
                    case FrameHeader.NAK:
                        return Frame.NAK;
                    case FrameHeader.CAN:
                        return Frame.CAN;
                    case FrameHeader.SOF:
                        return await ReadDataFrame(cancelation);
                }
            }
        }

        private async Task<DataFrame> ReadDataFrame(CancellationToken cancelation)
        {
            var length = await Stream.ReadByte(cancelation);

            var buffer = new List<byte> { length };
            buffer.AddRange(await Stream.Read(length + 1, cancelation));

            var actualChecksum = buffer[buffer.Count - 1];
            var expectedChecksum = buffer.Take(buffer.Count - 1).CalculateChecksum();
            if (actualChecksum != expectedChecksum)
                throw new ChecksumException("Checksum failure");

            var type = (DataFrameType)buffer[1];
            var function = (CommandFunction)buffer[2];
            var payload = buffer.Skip(3).Take(buffer.Count - 4).ToArray();

            return new DataFrame(type, function, payload);
        }
    }
}
