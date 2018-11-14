using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public class Message : IEquatable<Message>
    {
        public readonly ControllerFunction Function;
        public readonly byte[] Payload;

        public Message(ControllerFunction function, byte[] payload)
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

        public static bool operator ==(Message frame1, Message frame2)
        {
            return EqualityComparer<Message>.Default.Equals(frame1, frame2);
        }

        public static bool operator !=(Message frame1, Message frame2)
        {
            return !(frame1 == frame2);
        }

        public static implicit operator Message(DataFrame frame)
        {
            switch (frame.Type)
            {
                case DataFrameType.REQ:
                    return new RequestMessage((ControllerFunction)frame.Payload[0], frame.Payload.Skip(1).ToArray());
                case DataFrameType.RES:
                    return new ResponseMessage((ControllerFunction)frame.Payload[0], frame.Payload.Skip(1).ToArray());
            }
            throw new InvalidCastException();
        }

        public static implicit operator DataFrame(Message message)
        {
            switch (message)
            {
                case RequestMessage request:
                    return new DataFrame(DataFrameType.REQ, message.Payload);
                case ResponseMessage response:
                    return new DataFrame(DataFrameType.RES, message.Payload);
            }
            throw new InvalidCastException();
        }
    }

    public class RequestMessage : Message
    {
        public RequestMessage(ControllerFunction function, byte[] payload) : base(function, payload)
        {
        }
    }

    public class ResponseMessage : Message
    {
        public ResponseMessage(ControllerFunction function, byte[] payload) : base(function, payload)
        {
        }
    }
}
