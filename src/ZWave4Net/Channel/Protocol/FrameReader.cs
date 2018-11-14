using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

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
                cancelation.ThrowIfCancellationRequested();

                var header = await Stream.ReadHeader(cancelation);

                switch (header)
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
            // read the length
            var length = await Stream.ReadByte(cancelation);

            // read data (payload and checksum)
            var data = await Stream.Read(length, cancelation);

            // payload (data without checksum)
            var payload = data.Take(data.Length - 1).ToArray();

            // checksum
            var actualChecksum = data.Last();

            // calculate required checksum (include length)
            var expectedChecksum = new byte[] { length }.Concat(payload).CalculateChecksum();

            // validate checksum
            if (actualChecksum != expectedChecksum)
                throw new ChecksumException("Checksum failure");

            // return dataframe
            return new DataFrame((DataFrameType)payload[0], payload.Skip(1).ToArray());
        }
    }
}
