using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ZWave4Net.Channel.Protocol
{
    public static partial class Extentions
    {
        public static byte CalculateChecksum(this IEnumerable<byte> values)
        {
            return values.Aggregate((byte)0xFF, (total, next) => total ^= next);
        }

        public static Task WriteHeader(this IByteStream stream, FrameHeader header, CancellationToken cancelation)
        {
            return stream.Write(new[] { (byte)header }, cancelation);
        }

        public static async Task<byte> ReadByte(this IByteStream stream, CancellationToken cancelation)
        {
            return (await stream.Read(1, cancelation)).Single();
        }

        public static async Task<FrameHeader> ReadHeader(this IByteStream stream, CancellationToken cancelation)
        {
            return (FrameHeader)(await stream.Read(1, cancelation)).Single();
        }
    }
}
