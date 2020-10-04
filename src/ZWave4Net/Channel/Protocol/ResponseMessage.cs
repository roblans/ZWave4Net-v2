using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.Channel.Protocol
{
    /// <summary>
    /// Response from controller to host
    /// </summary>
    internal class ResponseMessage : Message
    {
        public ResponseMessage(Payload payload)
            : base(payload)
        {
        }

        public ResponseMessage()
            : this(Payload.Empty)
        {
        }

        public override string ToString()
        {
            return $"Response {base.ToString()}";
        }
    }
}

