using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.Channel.Protocol.Frames;

namespace ZWave4Net
{
    public class ZWaveController
    {
        internal readonly ZWaveChannel Channel;

        public uint HomeID { get; private set; }
        public byte NodeID { get; private set; }
        public string Version { get; private set; }
        public ZWaveChipType ChipType { get; private set; }

        public readonly NodeCollection Nodes = new NodeCollection();

        public ZWaveController(ISerialPort port)
        {
            if (port == null)
                throw new ArgumentNullException(nameof(port));

            Channel = new ZWaveChannel(port);
        }

#if NET472
        public ZWaveController(string portName)
        {
            if (portName == null)
                throw new ArgumentNullException(nameof(portName));

            var port = new SerialPort(portName);
            Channel = new ZWaveChannel(port);
        }
#endif

        internal static string GetEndpointName(byte nodeID, byte endpointID)
        {
            return endpointID != 0 ? $"{nodeID:D3}.{endpointID:D3}" : $"{nodeID:D3}";
        }

        public async Task Open(bool softReset = true)
        {
            await Channel.Open(softReset);

            var getVersion = await Channel.Send<Payload>(new ControllerRequest(Function.GetVersion));
            using (var reader = new PayloadReader(getVersion))
            {
                Version = reader.ReadString();
            }

            var memoryGetId = await Channel.Send<Payload>(new ControllerRequest(Function.MemoryGetId));
            using (var reader = new PayloadReader(memoryGetId))
            {
                HomeID = reader.ReadUInt32();
                NodeID = reader.ReadByte();
            }

            var discoveryNodes = await Channel.Send<Payload>(new ControllerRequest(Function.DiscoveryNodes));
            using (var reader = new PayloadReader(discoveryNodes))
            {
                var version = reader.ReadByte();
                var capabilities = reader.ReadByte();
                var length = reader.ReadByte();
                var nodes = reader.ReadBytes(length);

                var bits = new BitArray(nodes);
                for (byte i = 0; i < bits.Length; i++)
                {
                    if (bits[i])
                    {
                        var node = Factory.CreateNode(this, (byte)(i + 1));

                        await node.Initialize();

                        Nodes.Add(node);
                    }
                }

                ChipType = (ZWaveChipType)reader.ReadUInt16();
            }
        }

        public async Task Close()
        {
            await Channel.Close();
        }
    }
}
