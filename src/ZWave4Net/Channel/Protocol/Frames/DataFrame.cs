using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ZWave4Net.Channel.Protocol.Frames
{
    public class DataFrame : Frame, IEquatable<DataFrame>
    {
        public readonly DataFrameType Type;
        public readonly byte[] Payload;

        public DataFrame(DataFrameType type, byte[] payload)
            : base(FrameHeader.SOF)
        {
            Type = type;
            Payload = payload;
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
                   Payload.SequenceEqual(other.Payload);
        }

        public override int GetHashCode()
        {
            var hashCode = 876327360;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"{Header} {Type} {(ControllerFunction)Payload[0]} {BitConverter.ToString(Payload.Skip(1).ToArray())}";
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
