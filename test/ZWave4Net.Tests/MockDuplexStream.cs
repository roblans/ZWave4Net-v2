using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel;

namespace ZWave4Net.Tests
{
    public class MockDuplexStream : IDuplexStream
    {
        public EventHandler<EventArgs> AfterWrite;

        public readonly MemoryStream Input;
        public readonly MemoryStream Output;

        public MockDuplexStream()
        {
            Input = new MemoryStream();
            Output = new MemoryStream();
        }

       
        public async Task<byte[]> Read(int lenght, CancellationToken cancelation)
        {
            var buffer = new byte[lenght];

            var read = 0;
            while (read < lenght)
            {
                read += await Input.ReadAsync(buffer, read, lenght - read, cancelation);
            }

            return buffer;
        }

        public async Task Write(byte[] values, CancellationToken cancelation)
        {
            await Output.WriteAsync(values, 0, values.Length, cancelation);

            Debug.WriteLine($"Write: {BitConverter.ToString(values)}");

            AfterWrite?.Invoke(this, EventArgs.Empty);
        }
    }
}
