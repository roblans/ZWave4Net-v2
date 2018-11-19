using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Utilities;

namespace ZWave4Net.Tests
{
    public class MockDuplexStream : IDuplexStream
    {
        private readonly ILogger _logger = Logging.CreatLogger("MockDuplexStream");

        public EventHandler<EventArgs> AfterWrite;

        public readonly MemoryStream Input;
        public readonly MemoryStream Output;

        public MockDuplexStream()
        {
            Input = new MemoryStream();
            Output = new MemoryStream();
        }

       
        public async Task<byte[]> Read(int lenght, CancellationToken cancellation)
        {
            var buffer = new byte[lenght];

            var read = 0;
            while (read < lenght)
            {
                read += await Input.ReadAsync(buffer, read, lenght - read, cancellation);
            }

            return buffer;
        }

        public async Task Write(byte[] values, CancellationToken cancellation)
        {
            await Output.WriteAsync(values, 0, values.Length, cancellation);

            _logger.LogDebug($"Write {BitConverter.ToString(values)}");

#pragma warning disable 4014
            Task.Run(() => AfterWrite?.Invoke(this, EventArgs.Empty));
#pragma warning restore 4014
        }
    }
}
