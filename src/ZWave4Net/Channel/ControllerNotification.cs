using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{

    public class ControllerNotification
    {
        public TimeSpan ResponseTimeout = TimeSpan.FromSeconds(5);
        public int MaxRetryAttempts = 3;

        public bool UseCallbackID { get; set; }

        public readonly Function Function;
        public readonly IPayload Payload;

        public ControllerNotification(Function function, IPayload payload = null)
        {
            Function = function;
            Payload = payload;
        }
    }

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
