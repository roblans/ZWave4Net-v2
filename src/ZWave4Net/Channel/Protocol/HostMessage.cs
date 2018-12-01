using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    /// <summary>
    /// Message from host to controller
    /// </summary>
    public class HostMessage : Message
    {
        public TimeSpan Timeout = TimeSpan.FromSeconds(5);
        public int MaxRetryAttempts = 0;

        public HostMessage(Payload payload) : base(payload)
        {
        }

        public HostMessage() : this(Payload.Empty)
        {
        }

        public override string ToString()
        {
            return $"Request {base.ToString()}";
        }
    }
}
