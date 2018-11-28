using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Diagnostics;

namespace ZWave4Net.Channel.Protocol
{
    public abstract class Message : IEquatable<Message>
    {
        public readonly Function Function;
        public readonly byte[] Payload;

        protected Message(Function function, byte[] payload)
        {
            Function = function;
            Payload = payload;
        }

        public override string ToString()
        {
            return $"{Function} {BitConverter.ToString(Payload)}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Message);
        }

        public bool Equals(Message other)
        {
            return other != null &&
                   base.Equals(other) &&
                   GetType() == other.GetType() &&
                   Function == other.Function &&
                   Payload.SequenceEqual(other.Payload);
        }

        public override int GetHashCode()
        {
            var hashCode = -988694756;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Payload[0].GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Message frame1, Message frame2)
        {
            return EqualityComparer<Message>.Default.Equals(frame1, frame2);
        }

        public static bool operator !=(Message frame1, Message frame2)
        {
            return !(frame1 == frame2);
        }
    }
}
