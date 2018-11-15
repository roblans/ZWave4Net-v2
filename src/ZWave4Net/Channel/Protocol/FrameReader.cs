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
        public readonly IDuplexStream Stream;

        public FrameReader(IDuplexStream stream)
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
            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.2.1 Data frame reception timeout
            // A receiving host or Z-Wave chip MUST abort reception of a Data frame if the reception has lasted for 
            // more than 1500ms after the reception of the SOF byte.
            using (var timeoutCancelation = new CancellationTokenSource(1500))
            {
                // combine the passed and the timeout cancelationtokens  
                using (var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancelation, timeoutCancelation.Token))
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
    }
}
