using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{

    internal abstract class ControllerMessage : Message
    {
        public readonly Function Function;

        public ControllerMessage(Function function,  Payload payload)
            : base(payload)
        {
            Function = function;
        }
    }
}
