using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel
{
    internal interface IEncapsulatedCommand
    {
        Command Decapsulate();
    }
}
