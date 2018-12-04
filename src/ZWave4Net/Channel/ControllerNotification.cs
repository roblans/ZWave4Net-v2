using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{

    public abstract class ControllerNotification
    {
        public readonly Function Function;
        public readonly byte? CallbackID;
        public readonly Payload Payload;

        public ControllerNotification(Function function, byte? callbackID, Payload payload)
        {
            Function = function;
            CallbackID = callbackID;
            Payload = payload;
        }
    }
}
