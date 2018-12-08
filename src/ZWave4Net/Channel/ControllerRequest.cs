using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class ControllerRequest : ControllerMessage
    {
        public TimeSpan ResponseTimeout = TimeSpan.FromSeconds(60);
        public int MaxRetryAttempts = 3;

        public bool UseCallbackID { get; set; }

        public ControllerRequest(Function function, Payload payload)
            : base(function, payload)
        {
        }

        public ControllerRequest(Function function) : this(function, Payload.Empty)
        {
        }
    }
}
