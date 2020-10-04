using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ZWave.Channel.Protocol.Frames
{
    public static partial class Extentions
    {
        public static byte CalculateLrc8Checksum(this IEnumerable<byte> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            return values.Aggregate((byte)0xFF, (total, next) => total ^= next);
        }

        public static Task WriteHeader(this IDuplexStream stream, FrameHeader header, CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return stream.Write(new[] { (byte)header }, cancellationToken);
        }

        public static async Task<byte> ReadByte(this IDuplexStream stream, CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return (await stream.Read(1, cancellationToken)).Single();
        }

        public static async Task<FrameHeader> ReadHeader(this IDuplexStream stream, CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return (FrameHeader)(await stream.Read(1, cancellationToken)).Single();
        }
    }
}
