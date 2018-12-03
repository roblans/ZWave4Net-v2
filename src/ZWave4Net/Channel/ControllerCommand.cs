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

        public readonly Function Function;
        public readonly IPayload Payload;

        public ControllerCommand(Function function, IPayload payload = null)
        {
            Function = function;
            Payload = payload;
        }
    }
}
