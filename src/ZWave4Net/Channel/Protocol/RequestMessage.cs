using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.Channel.Protocol
{
    /// <summary>
    /// Message from host to controller
    /// </summary>
    internal class RequestMessage : Message
    {
        public RequestMessage(Payload payload)
            : base(payload)
        {
        }

        public RequestMessage() 
            : this(Payload.Empty)
        {
        }

        public override string ToString()
        {
            return $"Request {base.ToString()}";
        }
    }
}
