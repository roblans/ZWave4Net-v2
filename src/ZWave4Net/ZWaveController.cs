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
        private static readonly object _lock = new object();
        private static byte _callbackID = 0;
        private readonly ZWaveChannel _channel;

        public uint HomeID { get; private set; }
        public byte NodeID { get; private set; }
        public string Version { get; private set; }
        public ZWaveChipType ChipType { get; private set; }

        public ZWaveController(ISerialPort port)
        {
            _channel = new ZWaveChannel(port);
        }

        private static byte GetNextCallbackID()
        {
            lock (_lock) { return _callbackID = (byte)((_callbackID % 255) + 1); }
        }

        public async Task Open()
        {
            await _channel.Open();

            // small delay, otherwise lots of CAN's received during startup
            await Task.Delay(1500);

            var getVersion = await _channel.Send(new HostMessage(Function.GetVersion));
            using (var reader = new PayloadReader(getVersion.Payload))
            {
                Version = reader.ReadString();
            }

            var memoryGetId = await _channel.Send(new HostMessage(Function.MemoryGetId));
            using (var reader = new PayloadReader(memoryGetId.Payload))
            {
                HomeID = reader.ReadUInt32();
                NodeID = reader.ReadByte();
            }

            var discoveryNodes = await _channel.Send(new HostMessage(Function.DiscoveryNodes));
            using (var reader = new PayloadReader(discoveryNodes.Payload))
            {
                var version = reader.ReadByte();
                var capabilities = reader.ReadByte();
                var length = reader.ReadByte();
                var nodes = reader.ReadBytes(length);

                ChipType = (ZWaveChipType)reader.ReadUInt16();
            }

            var payload = new byte[] { 42, GetNextCallbackID() };
            var requestNodeNeighborUpdate = await _channel.Send(new HostMessage(Function.RequestNodeNeighborUpdate,  payload), (response) =>
            {
                return true;
            });
        }

        public async Task Close()
        {
            await _channel.Close();
        }
    }
}
