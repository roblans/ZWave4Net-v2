using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class Command
    {
        public TimeSpan? ResponseTimeout;
        public int? MaxRetryAttempts;
        public bool UseCallbackID { get; set; }

        public Function Function { get; private set; }
        public Payload Payload { get; private set; }

        public Command(Function function, Payload payload = null)
        {
            Function = function;
            Payload = payload;
        }
    }
}
