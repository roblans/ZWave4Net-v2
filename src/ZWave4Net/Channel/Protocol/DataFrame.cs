using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ZWave4Net.Channel.Protocol
{
    public abstract class DataFrame : Frame
    {
        public readonly ControllerFunction Function;
        public readonly byte[] Payload;

        protected DataFrame(ControllerFunction function, byte[] payload)
            : base(FrameHeader.SOF)
        {
            Function = function;
            Payload = payload;
        }
    }
}
