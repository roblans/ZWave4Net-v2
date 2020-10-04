using System;
using System.Collections.Generic;
using System.Text;
using ZWave.Channel.Protocol;

namespace ZWave.Channel
{
    internal class ControllerRequest : ControllerMessage
    {

        public ControllerRequest(Function function, Payload payload)
            : base(function, payload)
        {
        }

        public ControllerRequest(Function function)
            : this(function, Payload.Empty)
        {
        }
    }
}
