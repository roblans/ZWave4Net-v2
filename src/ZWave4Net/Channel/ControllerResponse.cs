using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public class ControllerResponse<T> : ControllerNotification<T> where T : IPayload, new()
    {
        public ControllerResponse(Function function, byte? callbackID, T payload)
            : base(function, callbackID, payload)
        {
        }
    }
}
