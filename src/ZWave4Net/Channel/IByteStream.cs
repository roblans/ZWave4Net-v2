using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.Channel
{
    public interface IByteStream
    {
        Task Write(byte[] values, CancellationToken cancelation);
        Task<byte[]> Read(int lenght, CancellationToken cancelation);
    }
}
