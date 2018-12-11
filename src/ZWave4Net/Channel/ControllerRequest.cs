using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class ControllerRequest : ControllerMessage
    {

        public ControllerRequest(Function function, Payload payload)
            : base(function, payload)
        {
        }

        public ControllerRequest(Function function) : this(function, Payload.Empty)
        {
        }
    }
}
