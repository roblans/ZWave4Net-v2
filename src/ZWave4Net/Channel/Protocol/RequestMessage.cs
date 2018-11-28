using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public class RequestMessage : Message
    {
        public TimeSpan Timeout = TimeSpan.FromSeconds(1);

        public RequestMessage(Function function, byte[] payload) : base(function, payload)
        {
        }

        public RequestMessage(Function function) : base(function, new byte[0])
        {
        }

        public override string ToString()
        {
            return $"Request {base.ToString()}";
        }
    }
}
