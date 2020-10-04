using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave
{
    public interface IPayloadSerializable
    {
        void Read(PayloadReader reader);
        void Write(PayloadWriter writer);
    }
}
