using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class ControllerEvent : ControllerNotification
    {
        public ControllerEvent(Function function, byte? callbackID, Payload payload)
            : base(function, callbackID, payload)
        {
        }
    }
}
