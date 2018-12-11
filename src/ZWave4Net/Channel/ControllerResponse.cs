using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public class ControllerResponse : ControllerMessage
    {
        public readonly byte? CallbackID;

        public ControllerResponse(Function function, byte? callbackID, Payload payload)
            : base(function, payload)
        {
            CallbackID = callbackID;
            if (function == Function.SendData)
            {

            }
        }
    }
}
