using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public class PayloadBytes : IPayloadSerializable
    {
        public static readonly PayloadBytes Empty = new PayloadBytes();

        private byte[] _values;

        public PayloadBytes() : this(new byte[0])
        {
        }

        public PayloadBytes(params byte[] values)
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

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            _values = reader.ReadBytes(reader.Length - reader.Position);
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            writer.WriteBytes(_values);
        }
    }
}
