using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Diagnostics;

namespace ZWave4Net.Channel.Protocol
{
    public abstract class Message : IEquatable<Message>
    {
        public readonly byte[] Payload;

        protected Message(byte[] payload)
        {
            Payload = payload;
        }

        public override string ToString()
        {
            return $"{(ControllerFunction)Payload[0]} {BitConverter.ToString(Payload.Skip(1).ToArray())}";
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

    public class RequestMessage : Message
    {
        public RequestMessage(byte[] payload) : base(payload)
        {
        }

        public override string ToString()
        {
            return $"Request {base.ToString()}";
        }
    }

    public class ResponseMessage : Message
    {
        public ResponseMessage(byte[] payload) : base(payload)
        {
        }

        public override string ToString()
        {
            return $"Response {base.ToString()}";
        }
    }

    public class EventMessage : Message
    {
        public EventMessage(byte[] payload) : base(payload)
        {
        }

        public override string ToString()
        {
            return $"Event {base.ToString()}";
        }
    }
}
