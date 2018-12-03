using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class ControllerEvent<T> : ControllerNotification<T> where T : IPayload, new()
    {
        public ControllerEvent(Function function, byte? callbackID, T payload)
            : base(function, callbackID, payload)
        {
        }
    }
}
