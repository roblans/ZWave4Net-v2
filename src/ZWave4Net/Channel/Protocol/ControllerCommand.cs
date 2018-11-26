using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public class ControllerCommand
    {
        public readonly ControllerFunction Function;
        public readonly byte[] Payload;

        public ControllerCommand(ControllerFunction function, byte[] payload)
        {
            Function = function;
            Payload = payload;
        }
    }
}
