using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ZWave4Net.Channel.Protocol
{
    public abstract class DataFrame : Frame, IEquatable<DataFrame>
    {
        public readonly ControllerFunction Function;
        public readonly byte[] Payload;

        protected DataFrame(ControllerFunction function, byte[] payload)
            : base(FrameHeader.SOF)
        {
            Function = function;
            Payload = payload;
        }

        public override string ToString()
        {
            return $"{Header} {Function} {BitConverter.ToString(Payload)}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DataFrame);
        }

        public bool Equals(DataFrame other)
        {
            return other != null &&
                   base.Equals(other) &&
                   Function == other.Function &&
                   Payload.SequenceEqual(other.Payload);
        }

        public override int GetHashCode()
        {
            var hashCode = -988694756;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
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
