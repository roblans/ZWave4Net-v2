using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public class ControllerResponse : ControllerNotification
    {
        public ControllerResponse(Function function, byte? callbackID, Payload payload)
            : base(function, callbackID, payload)
        {
        }
    }
}
