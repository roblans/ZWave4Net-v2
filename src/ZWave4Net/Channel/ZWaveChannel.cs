using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class ZWaveChannel
    {
        private readonly FrameBroker _frameBroker;

        private readonly CancellationTokenSource _cancellationSource;
        public readonly IByteStream Stream;
        
        public ZWaveChannel(IByteStream stream)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));

            _cancellationSource = new CancellationTokenSource();

            _frameBroker = new FrameBroker(stream, _cancellationSource.Token);
        }

        public void Open()
        {
            _frameBroker.Start();
        }

        public void Close()
        {
            _cancellationSource.Cancel();

            _frameBroker.Wait(TimeSpan.FromSeconds(1));
        }
    }
}
