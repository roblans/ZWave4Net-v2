﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave.Channel.Protocol.Frames
{
    internal class FrameWriter
    {
        public readonly IDuplexStream Stream;

        public FrameWriter(IDuplexStream stream)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public async Task Write(Frame frame, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            switch (frame.Header)
            {
                case FrameHeader.ACK:
                case FrameHeader.NAK:
                case FrameHeader.CAN:
                    await Stream.WriteHeader(frame.Header, cancellationToken);
                    break;
                case FrameHeader.SOF:
                    await Write((DataFrame)frame, cancellationToken);
                    break;
            }
        }

        private async Task Write(DataFrame frame, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            // Header | Length | Type | Payload | Checksum
            var length = 3 + frame.Payload.Length + 1;

            // allocate buffer
            var buffer = new byte[length];

            // 0 Header
            buffer[0] = (byte)frame.Header;

            // 1 Length (1 byte for type + length of payload + 1 byte for checksum)
            buffer[1] = (byte)(frame.Payload.Length + 2);

            // 2 Type
            buffer[2] = (byte)frame.Type;

            // 3 Payload 
            Array.Copy(frame.Payload.ToArray(), 0, buffer, 3, frame.Payload.Length);

            // checksum
            buffer[buffer.Length - 1] = buffer.Skip(1).Take(buffer.Length - 2).CalculateLrc8Checksum();

            // and write to stream
            await Stream.Write(buffer.ToArray(), cancellationToken);
        }
    }
}
