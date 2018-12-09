using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public class ByteArray : IPayload
    {
        public static readonly ByteArray Empty = new ByteArray();

        private byte[] _values;

        public ByteArray() : this(new byte[0])
        {
        }

        public ByteArray(params byte[] values)
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
