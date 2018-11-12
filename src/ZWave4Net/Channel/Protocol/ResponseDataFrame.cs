using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public class ResponseDataFrame : DataFrame
    {
        public ResponseDataFrame(ControllerFunction function, byte[] payload)
            : base(function, payload)
        {
        }
    }
}
