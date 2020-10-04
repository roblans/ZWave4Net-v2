using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using ZWave.Channel;

namespace ZWave
{
    public class PayloadReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly byte[] _buffer = new byte[256];

        public PayloadReader(Payload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            _stream = new MemoryStream(payload.ToArray());
        }

        public PayloadReader(IEnumerable<byte> bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            _stream = new MemoryStream(bytes.ToArray());
        }

        private void FillBuffer(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "length cannot be less than zero");

            if (length == 0)
                return;

            _stream.Read(_buffer, 0, length);
        }

        public int Length
        {
            get { return (int)_stream.Length; }
        }

        public int Position
        {
            get { return (int)_stream.Position; }
        }

        public void SkipBytes(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "count cannot be less than zero");

            _stream.Seek(count, SeekOrigin.Current);
        }

        public byte ReadByte()
        {
            FillBuffer(1);
            return _buffer[0];
        }

        public bool ReadBoolean()
        {
            FillBuffer(1);
            return _buffer[0] != 0x00;
        }

        public sbyte ReadSByte()
        {
            FillBuffer(1);
            return (sbyte)(_buffer[0]);
        }

        public short ReadInt16()
        {
            FillBuffer(2);
            return (short)(_buffer[0] << 8 | _buffer[1]);
        }

        public ushort ReadUInt16()
        {
            FillBuffer(2);
            return (ushort)(_buffer[0] << 8 | _buffer[1]);
        }

        public int ReadInt24()
        {
            FillBuffer(3);
            return (_buffer[0] << 16 | _buffer[1] << 8 | _buffer[2]);
        }

        public int ReadInt32()
        {
            FillBuffer(4);
            return (_buffer[0] << 24 | _buffer[1] << 16 | _buffer[2] << 8 | _buffer[3]);
        }

        public uint ReadUInt32()
        {
            FillBuffer(4);
            return (uint)(_buffer[0] << 24 | _buffer[1] << 16 | _buffer[2] << 8 | _buffer[3]);
        }

        public byte[] ReadBytes(int length)
        {
            FillBuffer(length);
            return _buffer.Take(length).ToArray();
        }

        public string ReadString()
        {
            var bytes = new List<byte>();
            while(true)
            {
                var b = ReadByte();
                if (b == 0)
                    break;

                bytes.Add(b);
            }
            return Encoding.ASCII.GetString(bytes.ToArray(), 0, bytes.Count);
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
