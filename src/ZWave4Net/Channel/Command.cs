using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class Command : IPayloadWriteable
    {
        private static readonly object _lock = new object();
        private static byte _callbackID = 0;

        public TimeSpan? ResponseTimeout;
        public int? MaxRetryAttempts;

        public Function Function { get; private set; }
        public byte[] Payload { get; private set; }
        public bool UseCallbackID { get; private set; }
        public byte? CallbackID { get; private set; }

        public Command(Function function, bool useCallbackID = false, params byte[] payload)
        {
            Function = function;
            Payload = payload;
            UseCallbackID = useCallbackID;
        }

        public Command(Function function, params byte[] payload)
            : this(function, false, payload)
        {
        }

        public Command(Function function)
            : this(function, false, new byte[0])
        {
        }

        public Command(Function function, bool useCallbackID = false)
            : this(function, useCallbackID, new byte[0])
        {
        }

        private static byte GetNextCallbackID()
        {
            lock (_lock) { return _callbackID = (byte)((_callbackID % 255) + 1); }
        }

        protected virtual void WritePayload(PayloadWriter writer)
        {
            if (Payload != null && Payload.Length > 0)
            {
                writer.WriteBytes(Payload);
            }
        }

        void IPayloadWriteable.WriteTo(PayloadWriter writer)
        {
            writer.WriteByte((byte)Function);

            WritePayload(writer);

            if (UseCallbackID)
            {
                CallbackID = GetNextCallbackID();
                writer.WriteByte(CallbackID.Value);
            }
        }
    }

    public class Command<T> : Command where T : IPayloadWriteable
    {
        public new T Payload { get; private set; }

        public Command(Function function, bool useCallbackID, T payload) : base(function, useCallbackID)
        {
            Payload = payload;
        }

        public Command(Function function, T payload) : this(function, false, payload)
        {
        }

        protected override void WritePayload(PayloadWriter writer)
        {
            base.WritePayload(writer);

             writer.WriteObject(Payload);
        }
    }

}
