using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    public interface IBasic
    {
        Task<byte> GetValue();
        Task SetValue(byte value);
    }
}
