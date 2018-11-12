using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    enum TransmitOptions : byte
    {
        Ack = 0x01,
        LowPower = 0x02,
        AutoRoute = 0x04,
        NoRoute = 0x10,
        Explore = 0x20,
    }
}
