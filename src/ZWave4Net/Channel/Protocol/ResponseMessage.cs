using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    /// <summary>
    /// Response from controller to host
    /// </summary>
    public class ResponseMessage : ControllerMessage
    {
        public ResponseMessage(Payload payload) : base(payload)
        {
        }

        public ResponseMessage() : this(Payload.Empty)
        {
        }

        public override string ToString()
        {
            return $"Response {base.ToString()}";
        }
    }
}

