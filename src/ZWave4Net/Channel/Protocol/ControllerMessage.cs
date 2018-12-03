using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    /// <summary>
    /// Response from controller to host
    /// </summary>
    public abstract class ControllerMessage : Message
    {
        public ControllerMessage(Payload payload) : base(payload)
        {
        }

        public ControllerMessage() : this(Payload.Empty)
        {
        }
    }
}
