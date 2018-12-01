using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.Channel.Protocol.Frames;

namespace ZWave4Net
{
    public class ZWaveController
    {
        internal readonly MessageChannel Channel;

        public uint HomeID { get; private set; }
        public byte NodeID { get; private set; }
        public string Version { get; private set; }
        public ZWaveChipType ChipType { get; private set; }
        public NodeCollection Nodes { get; private set; } = new NodeCollection();

        public ZWaveController(ISerialPort port)
        {
            Channel = new MessageChannel(port);
        }

        public async Task Open()
        {
            await Channel.Open();

            var getVersion = await Channel.Send(new RequestCommand(Function.GetVersion));
            using (var reader = new PayloadReader(getVersion))
            {
                Version = reader.ReadString();
            }

            var memoryGetId = await Channel.Send(new RequestCommand(Function.MemoryGetId));
            using (var reader = new PayloadReader(memoryGetId))
            {
                HomeID = reader.ReadUInt32();
                NodeID = reader.ReadByte();
            }

            var discoveryNodes = await Channel.Send(new RequestCommand(Function.DiscoveryNodes));
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
                        var node = new Node((byte)(i + 1), this);
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
