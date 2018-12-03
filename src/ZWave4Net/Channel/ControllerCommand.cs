using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class ControllerCommand
    {
        public TimeSpan ResponseTimeout = TimeSpan.FromSeconds(5);
        public int MaxRetryAttempts = 3;

        public bool UseCallbackID { get; set; }

        public Function Function { get; private set; }
        public IPayload Payload { get; private set; }

        public ControllerCommand(Function function, IPayload payload = null)
        {
            Function = function;
            Payload = payload;
        }
    }

    public class ControllerResponse<T> where T : IPayload, new()
    {
        public Function Function { get; private set; }
        public byte? CallbackID { get; private set; }
        public T Payload { get; private set; }

        public ControllerResponse(Function function, byte? callbackID, T payload)
        {
            Function = function;
            CallbackID = callbackID;
            Payload = payload;
        }
    }
}
