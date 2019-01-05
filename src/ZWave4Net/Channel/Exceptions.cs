using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel
{
    public class StreamClosedException : Exception
    {
        public StreamClosedException() : base("The stream unexpectedly closed.") { }
        public StreamClosedException(string message) : base(message) { }
        public StreamClosedException(string message, Exception inner) : base(message, inner) { }
    }

    public class Crc16ChecksumException : Exception
    {
        public Crc16ChecksumException() : base("CRC16 Checksum failure.") { }
        public Crc16ChecksumException(string message) : base(message) { }
        public Crc16ChecksumException(string message, Exception inner) : base(message, inner) { }
    }
}
