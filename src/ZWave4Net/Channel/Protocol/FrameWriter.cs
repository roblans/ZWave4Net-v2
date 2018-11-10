using System;
using System.Collections.Generic;
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
                    await Write((DataFrame)frame, cancelation);
                    break;
            }
        }

        private async Task Write(DataFrame frame, CancellationToken cancelation)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            // Header | Length | Type | Funtion | N Parameters | Checksum
            var length = 4 + frame.Parameters.Length + 1;

            // allocate buffer
            var buffer = new byte[length];

            // 0 Header
            buffer[0] = (byte)frame.Header;

            // 1 Length: Type + Funtion + Parameters
            buffer[1] = (byte)(1 + 1 + frame.Parameters.Length);

            // 2 Type
            buffer[2] = (byte)(frame.Type);

            // 3 Function
            buffer[3] = (byte)(frame.Function);

            // 4 Parameters 
            Array.Copy(frame.Parameters, 0, buffer, 4, frame.Parameters.Length);

            // checksum
            buffer[length - 1] = buffer.Skip(1).Take(length - 2).CalculateChecksum();

            // and write to stream
            await Stream.Write(buffer.ToArray(), cancelation);
        }
    }
}
