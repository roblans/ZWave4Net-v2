using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.Channel
{
    public interface IDuplexStream
    {
        Task Write(byte[] values, CancellationToken cancellationToken);
        Task<byte[]> Read(int length, CancellationToken cancellationToken);
    }
}
