using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public sealed class DataFrame : Frame
    {
        public readonly DataFrameType Type;
        public readonly CommandFunction Function;
        public readonly byte[] Parameters;

        public DataFrame(DataFrameType type, CommandFunction function, byte[] parameters) 
            : base(FrameHeader.SOF)
        {
            Type = type;
            Function = function;
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public DataFrame(DataFrameType type, CommandFunction function)
            : this(type, function, new byte[0])
        {
        }

        public override string ToString()
        {
            return $"{Header} {Type} {Function}";
        }
    }
}
