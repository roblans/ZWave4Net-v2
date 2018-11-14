using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.Channel.Protocol
{
    public class FrameWriter
    {
        public readonly IByteStream Stream;

        public FrameWriter(IByteStream stream)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public async Task Write(Frame frame, CancellationToken cancelation)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            switch (frame.Header)
            {
                case FrameHeader.ACK:
                case FrameHeader.NAK:
                case FrameHeader.CAN:
                    await Stream.WriteHeader(frame.Header, cancelation);
                    break;
                case FrameHeader.SOF:
                    await Write((RequestDataFrame)frame, cancelation);
                    break;
            }
        }

        private async Task Write(RequestDataFrame frame, CancellationToken cancelation)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            // get the payload from the frame
            var parameters = frame.Payload;

            // Header | Length | Type | Funtion | N Parameters | checksum
            var length = 4 + parameters.Length + 1;

            // allocate buffer
            var buffer = new byte[length];

            // 0 Header
            buffer[0] = (byte)frame.Header;

            // 1 Length (no header and checksum)
            buffer[1] = (byte)(length - 2);

            // 2 Type
            buffer[2] = (byte)(frame.Type);

            // 3 Function
            buffer[3] = (byte)(frame.Function);

            // 4 Parameters 
            Array.Copy(parameters, 0, buffer, 4, parameters.Length);

            // checksum
            buffer[buffer.Length - 1] = buffer.Skip(1).Take(buffer.Length - 2).CalculateChecksum();

            // and write to stream
            await Stream.Write(buffer.ToArray(), cancelation);
        }
    }
}
