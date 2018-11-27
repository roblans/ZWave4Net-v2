using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net
{
    public class ZWaveController
    {
        private readonly ZWaveChannel _channel;

        public uint HomeID { get; private set; }
        public byte NodeID { get; private set; }

        public ZWaveController(ISerialPort port)
        {
            _channel = new ZWaveChannel(port);
        }

        public async Task Open()
        {
            await _channel.Open();

            var result = await _channel.Send(new RequestMessage(Function.MemoryGetId));
            using (var reader = new PayloadReader(result.Payload))
            {
                HomeID = reader.ReadUInt32();
                NodeID = reader.ReadByte();
            }
        }

        public async Task Close()
        {
            await _channel.Close();
        }
    }
}
