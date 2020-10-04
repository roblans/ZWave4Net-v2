using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.CommandClasses
{
    public enum Powerlevel : byte
    {
        NormalPower = 0,
        Minus1dBm = 1,
        Minus2dBm = 2,
        Minus3dBm = 3,
        Minus4dBm = 4,
        Minus5dBm = 5,
        Minus6dBm = 6,
        Minus7dBm = 7,
        Minus8dBm = 8,
        Minus9dBm = 9,
    }
}
