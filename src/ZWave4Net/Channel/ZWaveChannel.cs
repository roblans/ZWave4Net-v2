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
        private readonly ControllerCommandDispatcher _dispatcher;
        public readonly ISerialPort Port;
        
        public ZWaveChannel(ISerialPort port)
        {
            Port = port ?? throw new ArgumentNullException(nameof(port));

            _dispatcher = new ControllerCommandDispatcher(port);
        }

        public async Task Open()
        {
            await Port.Open();
            await _dispatcher.Initialize();
        }

        public async Task Close()
        {
            await _dispatcher.Shutdown();
            await Port.Close();
        }
    }
}
