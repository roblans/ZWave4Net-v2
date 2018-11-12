using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class ZWaveChannel
    {
        public readonly IByteStream Stream;
        
        public ZWaveChannel(IByteStream stream)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        private void Send(DataFrame frame)
        {
        }

        public void Send(byte nodeID, object command)
        {
        }
    }
}
