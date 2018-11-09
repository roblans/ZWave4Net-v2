using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public sealed class DataFrame : Frame, IEquatable<DataFrame>
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

        public override bool Equals(object obj)
        {
            return Equals(obj as DataFrame);
        }

        public bool Equals(DataFrame other)
        {
            return other != null &&
                   base.Equals(other) &&
                   Type == other.Type &&
                   Function == other.Function &&
                   Parameters.SequenceEqual(other.Parameters);
        }

        public override int GetHashCode()
        {
            var hashCode = -2001256323;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + Function.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(DataFrame frame1, DataFrame frame2)
        {
            return EqualityComparer<DataFrame>.Default.Equals(frame1, frame2);
        }

        public static bool operator !=(DataFrame frame1, DataFrame frame2)
        {
            return !(frame1 == frame2);
        }
    }
}
