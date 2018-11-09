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
            switch(frame.Header)
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
            // Header | Length | Type | Function
            var buffer = new List<byte> { (byte)frame.Header, 0x00, (byte)frame.Type, (byte)frame.Function };

            // Header | Length | Type | Function | Parameter1 .. ParameterN
            buffer.AddRange(frame.Parameters);

            // patch length without Header
            buffer[1] = (byte)(buffer.Count - 1);

            // add checksum (skip header)
            buffer.Add(buffer.Skip(1).CalculateChecksum());

            // and write to stream
            await Stream.Write(buffer.ToArray(), cancelation);
        }
    }
}
