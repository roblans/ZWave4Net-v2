using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public interface ICommand
    {
       Function Function { get; }
       byte? CallbackID { get; }
    }

    public interface IRequestCommand : ICommand, IPayloadWriteable
    {
        bool UseCallbackID { get; }
    }

    public interface IResponseCommand : ICommand, IPayloadReadable
    {
        bool UseCallbackID { set; }
    }

    public class RequestCommand<T> : IRequestCommand where T : IPayloadWriteable
    {
        private static readonly object _lock = new object();
        private static byte _callbackID = 0;

        public Function Function { get; private set; }
        public T Payload { get; private set; }
        public bool UseCallbackID { get; private set; }
        public byte? CallbackID { get; private set; }

        public RequestCommand(Function function, T payload, bool useCallbackID = false)
        {
            Function = function;
            Payload = payload;
            UseCallbackID = useCallbackID;
        }

        public RequestCommand(Function function, bool useCallbackID) : this(function, default(T), useCallbackID)
        {
        }

        private static byte GetNextCallbackID()
        {
            lock (_lock) { return _callbackID = (byte)((_callbackID % 255) + 1); }
        }

        public void WriteTo(PayloadWriter writer)
        {
            writer.WriteByte((byte)Function);

            if (!object.Equals(Payload, default(T)))
            {
                writer.WriteObject(Payload);
            }

            if (UseCallbackID)
            {
                CallbackID = GetNextCallbackID();
                writer.WriteByte(CallbackID.Value);
            }
        }
    }

    public class RequestCommand : RequestCommand<Payload>
    {
        public RequestCommand(Function function, bool useCallbackID = false)
            : base(function, useCallbackID)
        {
        }

        public RequestCommand(Function function, Payload payload, bool useCallbackID = false)
            : base(function, payload, useCallbackID)
        {
        }
    }

    public class ResponseCommand<T> : IResponseCommand where T : IPayloadReadable, new()
    {
        public Function Function { get; private set; }
        public T Payload { get; private set; }
        public bool UseCallbackID { get; set; }
        public byte? CallbackID { get; private set; }

        public override string ToString()
        {
            return $"Response {Function} {Payload}";
        }

        public void ReadFrom(PayloadReader reader)
        {
            Function = (Function)reader.ReadByte();

            if (UseCallbackID)
            {
                CallbackID = reader.ReadByte();
            }

            if (reader.Position < reader.Length)
            {
                Payload = reader.ReadObject<T>();
            }
        }
    }

    public class ResponseCommand : ResponseCommand<Payload>
    {
    }

    public class Payload : IPayloadReadable, IPayloadWriteable
    {
        public static readonly Payload Empty = new Payload();

        private byte[] _values;

        public Payload() : this(new byte[0])
        {
        }

        public Payload(params byte[] values)
        {
            _values = values ?? new byte[0];
        }

        public int Length
        {
            get { return _values.Length; }
        }

        public byte[] ToArray()
        {
            return _values;
        }

        public void ReadFrom(PayloadReader reader)
        {
            _values = reader.ReadBytes(reader.Length - reader.Position);
        }

        public void WriteTo(PayloadWriter writer)
        {
            writer.WriteBytes(_values);
        }
    }
}
