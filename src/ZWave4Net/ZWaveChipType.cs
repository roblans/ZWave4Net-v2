using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 7.4 Node List Command 
    public enum ZWaveChipType
    {
        ZW0102 = 0x0102, 
        ZW0201 = 0x0201,
        ZW0301 = 0x0301,
        ZM0401 = 0x0401, 
        ZM4102 = 0x0401, 
        SD3402 = 0x0401, 
        ZW050x = 0x0500,
    }
}
