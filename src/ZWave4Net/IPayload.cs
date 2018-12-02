using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public interface IPayload
    {
        void Read(PayloadReader reader);
        void Write(PayloadWriter writer);
    }
}
