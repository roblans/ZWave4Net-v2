using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public class Payload : IPayload
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

        public byte this[int index]
        {
            get { return _values[index]; }
        }

        public byte[] ToArray()
        {
            return _values;
        }

        public override string ToString()
        {
            return BitConverter.ToString(_values);
        }

        void IPayload.Read(PayloadReader reader)
        {
            _values = reader.ReadBytes(reader.Length - reader.Position);
        }

        void IPayload.Write(PayloadWriter writer)
        {
            writer.WriteBytes(_values);
        }
    }
}
