using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public class EventDataFrame : ResponseDataFrame
    {
        public EventDataFrame(ControllerFunction function, byte[] payload)
            : base(function, payload)
        {
        }
    }
}
