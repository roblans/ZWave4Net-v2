using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net
{
    public class ZWaveController
    {
        private static readonly object _lock = new object();
        private static byte _callbackID = 0;
        private readonly MessageChannel _channel;

        public uint HomeID { get; private set; }
        public byte NodeID { get; private set; }
        public string Version { get; private set; }
        public ZWaveChipType ChipType { get; private set; }

        public ZWaveController(ISerialPort port)
        {
            _channel = new MessageChannel(port);
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

        }

        public async Task<NeighborUpdateStatus> RequestNeighborUpdate(byte nodeID, Action<NeighborUpdateStatus> onProgress, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var writer = new PayloadWriter())
            {
                // get next callbackID (1..255) 
                var callbackID = GetNextCallbackID();

                // write the ID of the node
                writer.WriteByte(nodeID);
                
                // write the callback
                writer.WriteByte(callbackID);

                // build the host message
                var message = new HostMessage(Function.RequestNodeNeighborUpdate, writer.GetPayload());

                // send request
                var requestNodeNeighborUpdate = await _channel.Send(message, (response) =>
                {
                    // response received, so open a reader
                    using (var reader = new PayloadReader(response.Payload))
                    {
                        // check if callback matches request 
                        if (reader.ReadByte() == callbackID)
                        {
                            // yes, so read status
                            var status = (NeighborUpdateStatus)reader.ReadByte();

                            // if callback delegate provided then invoke with progress 
                            onProgress?.Invoke(status);

                            // return true when final state reached (we're done)
                            return status == NeighborUpdateStatus.Done || status == NeighborUpdateStatus.Failed;
                        }
                    }
                    return false;
                }, cancellationToken);

                using (var reader = new PayloadReader(requestNodeNeighborUpdate.Payload))
                {
                    // skip the callback
                    reader.SkipBytes(1);

                    // return the status of the final response
                    return (NeighborUpdateStatus)reader.ReadByte();
                }
            }
        }

        public async Task Close()
        {
            await _channel.Close();
        }
    }
}
