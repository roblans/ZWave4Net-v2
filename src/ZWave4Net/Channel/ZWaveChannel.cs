using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class ZWaveChannel
    {
        private readonly MessageBroker _frameBroker;

        private readonly CancellationTokenSource _cancellationSource;
        public readonly ISerialPort Port;
        
        public ZWaveChannel(ISerialPort port)
        {
            Port = port ?? throw new ArgumentNullException(nameof(port));

            _cancellationSource = new CancellationTokenSource();

            _frameBroker = new MessageBroker(port);
        }

        public async Task Open()
        {
            await Port.Open();

            _frameBroker.Run(_cancellationSource.Token);
        }

        public async Task Close()
        {
            _cancellationSource.Cancel();

            await _frameBroker;
            await Port.Close();
        }
    }
}
