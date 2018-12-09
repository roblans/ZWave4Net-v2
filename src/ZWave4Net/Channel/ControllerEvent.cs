using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class ControllerEvent : ControllerMessage
    {
        public readonly byte? CallbackID;

        public ControllerEvent(Function function, byte? callbackID, PayloadBytes payload)
            : base(function, payload)
        {
            CallbackID = callbackID;
        }
    }
}
