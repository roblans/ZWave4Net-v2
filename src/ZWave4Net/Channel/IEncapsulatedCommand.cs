using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.Channel
{
    internal interface IEncapsulatedCommand
    {
        Command Decapsulate();
    }
}
