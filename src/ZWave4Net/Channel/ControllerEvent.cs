using System;
using System.Collections.Generic;
using System.Text;
using ZWave.Channel.Protocol;

namespace ZWave.Channel
{
    internal class ControllerEvent : ControllerMessage
    {
        public readonly byte? CallbackID;

        public ControllerEvent(Function function, byte? callbackID, Payload payload)
            : base(function, payload)
        {
            CallbackID = callbackID;
        }
    }
}
