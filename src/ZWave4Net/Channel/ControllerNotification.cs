using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{

    public abstract class ControllerNotification<T> where T : IPayload, new()
    {
        public readonly Function Function;
        public readonly byte? CallbackID;
        public readonly T Payload;

        public ControllerNotification(Function function, byte? callbackID, T payload)
        {
            Function = function;
            CallbackID = callbackID;
            Payload = payload;
        }
    }
}
