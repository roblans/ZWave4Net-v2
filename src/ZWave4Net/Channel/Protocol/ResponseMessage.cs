using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public class ResponseMessage : Message
    {
        public ResponseMessage(Function function, byte[] payload) : base(function, payload)
        {
        }

        public override string ToString()
        {
            return $"Response {base.ToString()}";
        }
    }
}
