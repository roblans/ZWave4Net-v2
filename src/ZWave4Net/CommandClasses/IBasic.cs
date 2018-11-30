using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public interface IBasic
    {
        byte GetValue();
        void SetValue();
    }
}
