using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public interface IPayloadWriteable
    {
        void WriteTo(PayloadWriter writer);
    }
}
