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
            var buffer = await Stream.Read(length, cancelation);

            return new DataFrame((DataFrameType)buffer[0], (CommandFunction)buffer[1], buffer.Skip(2).ToArray());
        }
    }
}
