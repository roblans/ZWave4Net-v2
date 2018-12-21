using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace ZWave4Net.Channel.Protocol.Frames
{
    internal class FrameReader
    {
        public readonly IDuplexStream Stream;

        public FrameReader(IDuplexStream stream)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public async Task<Frame> Read(CancellationToken cancellationToken = default(CancellationToken))
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var header = await Stream.ReadHeader(cancellationToken);

                switch (header)
                {
                    case FrameHeader.ACK:
                        return Frame.ACK;
                    case FrameHeader.NAK:
                        return Frame.NAK;
                    case FrameHeader.CAN:
                        return Frame.CAN;
                    case FrameHeader.SOF:
                        return await ReadDataFrame(cancellationToken);
                }
            }
        }

        private async Task<DataFrame> ReadDataFrame(CancellationToken cancellationToken = default(CancellationToken))
        {
            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.2.1 Data frame reception timeout
            // A receiving host or Z-Wave chip MUST abort reception of a Data frame if the reception has lasted for 
            // more than 1500ms after the reception of the SOF byte.
            using (var timeoutCancelation = new CancellationTokenSource(ProtocolSettings.DataFrameWaitTime))
            {
                // combine the passed and the timeout cancelationtokens  
                using (var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancelation.Token))
                {
                    // read the length
                    var length = await Stream.ReadByte(linkedCancellation.Token);

                    // read data (payload and checksum)
                    var data = await Stream.Read(length, linkedCancellation.Token);

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
                    return new DataFrame((DataFrameType)payload[0], new Payload(payload.Skip(1)));
                }
            }
        }
    }
}
