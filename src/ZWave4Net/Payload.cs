using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace ZWave4Net
{
    public class Payload : IEnumerable<byte>, IPayloadSerializable
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

        public Payload(IEnumerable<byte> values)
        {
            _values = values?.ToArray() ?? new byte[0];
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

        public IEnumerator<byte> GetEnumerator()
        {
            return ((IEnumerable<byte>)_values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<byte>)_values).GetEnumerator();
        }
    }
}
