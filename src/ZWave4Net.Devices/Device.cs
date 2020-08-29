using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Devices
{
    public class Device : IDevice
    {
        public Node Node { get; }

        public Device(Node node)
        {
            Node = node;
        }
    }
}
