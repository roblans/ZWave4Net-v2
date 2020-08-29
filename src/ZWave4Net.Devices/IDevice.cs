using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Devices
{
    public interface IDevice
    {
        Node Node { get; }
    }
}
