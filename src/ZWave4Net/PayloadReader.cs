using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZWave4Net
{
    public class PayloadReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly byte[] _buffer = new byte[256];

        public PayloadReader(byte[] payload)
        {
            _stream = new MemoryStream(payload);
        }

        private void FillBuffer(int length)
        {
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

        public byte ReadByte()
        {
            FillBuffer(1);
            return _buffer[0];
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

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
