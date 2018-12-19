using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Diagnostics;

namespace ZWave4Net.Channel.Protocol
{
    public abstract class Message : IEquatable<Message>
    {
        public readonly Payload Payload;

        protected Message(Payload payload)
        {
            Payload = payload ?? throw new ArgumentNullException(nameof(Payload));
        }

        public override string ToString()
        {
            return $"{Payload}";
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
                   object.Equals(Payload, other.Payload);
        }

        public override int GetHashCode()
        {
            var hashCode = -988694756;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Payload.GetHashCode();
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
