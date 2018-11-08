using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public class DataFrame : Frame
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

        protected override byte[] GetPayload()
        {
            // SOF | Length | Type | Function
            var buffer = new List<byte> { (byte)FrameHeader.SOF, 0x00, (byte)Type, (byte)Function };

            // SOF | Length | Type | Function | Parameter1 .. ParameterN
            buffer.AddRange(Parameters);

            // patch length without SOF
            buffer[1] = (byte)(buffer.Count - 1);
            
            // add checksum 
            buffer.Add(buffer.Skip(1).Aggregate((byte)0xFF, (total, next) => total ^= next));

            // done, return payload 
            return buffer.ToArray();
        }

        public override string ToString()
        {
            return $"{Header} {Type} {Function}";
        }
    }
}
