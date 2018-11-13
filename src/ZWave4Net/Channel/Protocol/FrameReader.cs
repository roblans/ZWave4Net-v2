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
                }

                if (header == FrameHeader.SOF)
                {
                    try
                    {
                        return await ReadDataFrame(cancelation);
                    }
                    catch (ChecksumException)
                    {
                        // probally partially received frame, skip and wait for next frame
                        continue;
                    }
                }
                else
                {
                    throw new UnknownFrameException($"Unknown frame header: '{header}'");
                }
            }
        }

        private async Task<DataFrame> ReadDataFrame(CancellationToken cancelation)
        {
            // read the length
            var length = await Stream.ReadByte(cancelation);

            // Length + Type | Funtion | N Parameters | Checksum
            var data = new byte[] { length }.Concat(await Stream.Read(length + 1, cancelation)).ToArray();

            // 1 Type
            var type = (DataFrameType)data[1];

            // 2 Function
            var function = (ControllerFunction)data[2];

            // 3 Parameters
            var payload = data.Skip(3).Take(data.Length - 4).ToArray();

            // checksum
            var actualChecksum = data[data.Length - 1];

            // calculate required checksum
            var expectedChecksum = data.Take(data.Length - 1).CalculateChecksum();

            // validate checksum
            if (actualChecksum != expectedChecksum)
                throw new ChecksumException("Checksum failure");

            // create and return frame
            switch (type)
            {
                case DataFrameType.REQ:
                    return new RequestDataFrame(function, payload);
                case DataFrameType.RES:
                    return new ResponseDataFrame(function, payload);
                default:
                    throw new UnknownFrameException($"Unknown data frame type: '{type}'");
            }
        }
    }
}
