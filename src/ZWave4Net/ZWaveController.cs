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
    /// <summary>
    /// A controller is a Z-Wave device that has a full routing table and is therefore able to communicate with all nodes in the Z-Wave network
    /// </summary>
    public class ZWaveController
    {
        internal readonly ZWaveChannel Channel;

        /// <summary>
        /// An unique identifier to separate networks from each other
        /// </summary>
        public uint HomeID { get; private set; }

        /// <summary>
        /// The NodeID of the controller
        /// </summary>
        public byte NodeID { get; private set; }

        /// <summary>
        /// The version of the controller
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// The ChipType of the controller
        /// </summary>
        public ZWaveChipType ChipType { get; private set; }

        /// <summary>
        /// The collection of nodes
        /// </summary>
        public readonly NodeCollection Nodes = new NodeCollection();

        /// <summary>
        /// Initializes an new instance of the ZWaveController
        /// </summary>
        /// <param name="port">The serial port to use</param>
        public ZWaveController(ISerialPort port)
        {
            if (port == null)
                throw new ArgumentNullException(nameof(port));

            Channel = new ZWaveChannel(port);
        }

#if NETFRAMEWORK
        /// <summary>
        /// Initializes an new instance of the ZWaveController
        /// </summary>
        /// <param name="usbStick">The USB stick to use</param>
        public ZWaveController(UsbStick usbStick)
        {
            if (usbStick == null)
                throw new ArgumentNullException(nameof(usbStick));

            var port = new SerialPort(usbStick);
            Channel = new ZWaveChannel(port);
        }

        /// <summary>
        /// Initializes an new instance of the ZWaveController
        /// </summary>
        /// <param name="serialPortName">The name of the serial port to use</param>
        public ZWaveController(string serialPortName)
        {
            if (serialPortName == null)
                throw new ArgumentNullException(nameof(serialPortName));

            var port = new SerialPort(serialPortName);
            Channel = new ZWaveChannel(port);
        }
#endif

        /// <summary>
        /// Opens the controller
        /// </summary>
        /// <param name="softReset">True to perform a sofreset, False otherwise</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task Open(bool softReset = false)
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
                        var node = Factory.CreateNode((byte)(i + 1), this);

                        await node.Initialize();

                        Nodes.Add(node);
                    }
                }

                ChipType = (ZWaveChipType)reader.ReadUInt16();
            }
        }

        /// <summary>
        /// Closes the controller
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task Close()
        {
            await Channel.Close();
        }
    }
}
