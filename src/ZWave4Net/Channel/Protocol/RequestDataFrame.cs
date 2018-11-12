using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public class RequestDataFrame : DataFrame
    {
        public RequestDataFrame(ControllerFunction function, byte[] payload)
            : base(function, payload)
        {
        }

        public RequestDataFrame(ControllerFunction function)
            : this(function, new byte[0])
        {
        }
    }
}
