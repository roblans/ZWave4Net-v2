using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    [Flags()]
    enum ReceiveStatus : byte
    {
        None = 0x00,
        RoutedBusy = 0x01,
        LowPower = 0x02,
        TypeSingle = 0x04,
        TypeBroad = 0x08,
        TypeMulti = 0x10,
        TypeExplore = 0x20,
        ForeignFrame = 0x40,
    };
}
