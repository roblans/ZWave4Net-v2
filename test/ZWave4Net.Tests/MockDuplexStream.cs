using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Diagnostics;

namespace ZWave4Net.Tests
{
    public class MockDuplexStream : IDuplexStream
    {
        private readonly ILogger _logger = Logging.Factory.CreatLogger("MockDuplexStream");
        public EventHandler<EventArgs> AfterWrite;

        public readonly MemoryStream Input;
        public readonly MemoryStream Output;

        public MockDuplexStream()
        {
            Input = new MemoryStream();
            Output = new MemoryStream();
        }

       
        public async Task<byte[]> Read(int length, CancellationToken cancellationToken)
        {
            var buffer = new byte[length];

            var read = 0;
            while (read < length)
            {
                read += await Input.ReadAsync(buffer, read, length - read, cancellationToken);
            }

            _logger.LogDebug($"Read {BitConverter.ToString(buffer)}");

            return buffer;
        }

        public async Task Write(byte[] values, CancellationToken cancellationToken)
        {
            await Output.WriteAsync(values, 0, values.Length, cancellationToken);

            _logger.LogDebug($"Write {BitConverter.ToString(values)}");

#pragma warning disable 4014
            Task.Run(() => AfterWrite?.Invoke(this, EventArgs.Empty));
#pragma warning restore 4014
        }
    }
}
