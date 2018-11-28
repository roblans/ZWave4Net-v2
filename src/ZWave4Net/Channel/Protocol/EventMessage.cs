using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    /// <summary>
    /// Event from controller to host
    /// </summary>
    public class EventMessage : ControllerMessage
    {
        public EventMessage(Function function, byte[] payload) : base(function, payload)
        {
        }

        public override string ToString()
        {
            return $"Event {base.ToString()}";
        }
    }
}
