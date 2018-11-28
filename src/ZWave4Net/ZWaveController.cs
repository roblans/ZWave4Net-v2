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
        public string Version { get; private set; }
        public ZWaveChipType ChipType { get; private set; }

        public ZWaveController(ISerialPort port)
        {
            _channel = new ZWaveChannel(port);
        }

        public async Task Open()
        {
            await _channel.Open();

            var getVersion = await _channel.Send(new RequestMessage(Function.GetVersion));
            using (var reader = new PayloadReader(getVersion.Payload))
            {
                Version = reader.ReadString();
            }

            var memoryGetId = await _channel.Send(new RequestMessage(Function.MemoryGetId));
            using (var reader = new PayloadReader(memoryGetId.Payload))
            {
                HomeID = reader.ReadUInt32();
                NodeID = reader.ReadByte();
            }

            var discoveryNodes = await _channel.Send(new RequestMessage(Function.DiscoveryNodes));
            using (var reader = new PayloadReader(discoveryNodes.Payload))
            {
                var version = reader.ReadByte();
                var capabilities = reader.ReadByte();
                var length = reader.ReadByte();
                var nodes = reader.ReadBytes(length);

                ChipType = (ZWaveChipType)reader.ReadUInt16();
            }
        }

        public async Task Close()
        {
            await _channel.Close();
        }
    }
}
