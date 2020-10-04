using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.Channel.Protocol
{
    internal class ControllerResponse : ControllerMessage
    {
        public readonly byte? CallbackID;

        public ControllerResponse(Function function, byte? callbackID, Payload payload)
            : base(function, payload)
        {
            CallbackID = callbackID;
        }
    }
}
