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
    public class MockDuplexStream : IDuplexStream
    {
        private readonly MemoryStream _stream;

        public MockDuplexStream()
        {
            _stream = new MemoryStream();
        }

        public void ResetPosition()
        {
            _stream.Position = 0;
        }

        public byte[] ToArray()
        {
            return _stream.ToArray();
        }

        public void Write(byte[] values)
        {
            _stream.Write(values, 0, values.Length);
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
