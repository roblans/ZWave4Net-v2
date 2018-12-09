using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    /// <summary>
    /// Response from controller to host
    /// </summary>
    public class ResponseMessage : Message
    {
        public ResponseMessage(ByteArray payload) : base(payload)
        {
        }

        public ResponseMessage() : this(ByteArray.Empty)
        {
        }

        public override string ToString()
        {
            return $"Response {base.ToString()}";
        }
    }
}

