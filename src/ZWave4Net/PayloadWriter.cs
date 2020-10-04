using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using ZWave.Channel;

namespace ZWave
{
    public class PayloadWriter : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly byte[] _buffer = new byte[256];

        public PayloadWriter()
        {
            _stream = new MemoryStream();
        }

        public int Length
        {
            get { return (int)_stream.Length; }
        }

        public int Position
        {
            get { return (int)_stream.Position; }
        }

        public Payload ToPayload()
        {
            return new Payload(_stream.ToArray());
        }

        public byte[] ToByteArray()
        {
            return _stream.ToArray();
        }

        public void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        public void WriteBoolean(bool value)
        {
            _stream.WriteByte((byte)(value ? 0xFF : 0x00));
        }

        public void WriteSByte(sbyte value)
        {
            _stream.WriteByte((byte)value);
        }

        public void WriteInt16(short value)
        {
            _buffer[0] = (byte)(value >> 8);
            _buffer[1] = (byte)value;
            _stream.Write(_buffer, 0, 2);
        }

        public void WriteUInt16(ushort value)
        {
            _buffer[0] = (byte)(value >> 8);
            _buffer[1] = (byte)value;
            _stream.Write(_buffer, 0, 2);
        }

        public void WriteInt24(int value)
        {
            _buffer[0] = (byte)(value >> 16);
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)value;
            _stream.Write(_buffer, 0, 3);
        }

        public void WriteInt32(int value)
        {
            _buffer[0] = (byte)(value >> 24);
            _buffer[1] = (byte)(value >> 16);
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)value;
            _stream.Write(_buffer, 0, 4);
        }

        public void WriteUInt32(uint value)
        {
            _buffer[0] = (byte)(value >> 24);
            _buffer[1] = (byte)(value >> 16);
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)value;
            _stream.Write(_buffer, 0, 4);
        }

        public void WriteBytes(byte[] values)
        {
            _stream.Write(values, 0, values.Length);
        }

        public void WritePayload(Payload value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _stream.Write(value.ToArray(), 0, value.Length);
        }

        public void WriteString(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var bytes = Encoding.ASCII.GetBytes(value).Concat(new[] { (byte)0 }).ToArray();
            _stream.Write(bytes, 0, bytes.Length);
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
