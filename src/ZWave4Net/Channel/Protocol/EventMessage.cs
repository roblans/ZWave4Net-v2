using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.Channel.Protocol
{
    /// <summary>
    /// Event from controller to host
    /// </summary>
    internal class EventMessage : Message
    {
        public EventMessage(Payload payload)
            : base(payload)
        {
        }

        public override string ToString()
        {
            return $"Event {base.ToString()}";
        }
    }
}
