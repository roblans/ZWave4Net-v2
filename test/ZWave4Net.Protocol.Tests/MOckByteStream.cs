using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel;

namespace ZWave4Net.Protocol.Tests
{
    public class MockByteStream : IByteStream
    {
        private readonly Stream _stream;

        public MockByteStream()
        {
            _stream = new MemoryStream();
        }

        public void ResetPosition()
        {
            _stream.Position = 0;
        }

        public async Task<byte[]> Read(int lenght, CancellationToken cancelation)
        {
            var buffer = new byte[lenght];

            var read = 0;
            while (read < lenght)
            {
                read += await _stream.ReadAsync(buffer, read, lenght - read, cancelation);
            }

            return buffer;
        }

        public Task Write(byte[] values, CancellationToken cancelation)
        {
            return _stream.WriteAsync(values, 0, values.Length, cancelation);
        }
    }
}
