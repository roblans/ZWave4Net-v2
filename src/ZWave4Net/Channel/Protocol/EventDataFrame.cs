using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public class EventDataFrame : DataFrame
    {
        public EventDataFrame(ControllerFunction function, byte[] payload)
            : base(function, payload)
        {
        }

        public EventDataFrame(ControllerFunction function)
            : this(function, new byte[0])
        {
        }
    }
}
