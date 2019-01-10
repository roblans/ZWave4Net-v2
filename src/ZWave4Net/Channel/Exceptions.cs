using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol.Frames;

namespace ZWave4Net.Channel
{
    public class StreamClosedException : Exception
    {
        public StreamClosedException() : base("The stream unexpectedly closed.") { }
        public StreamClosedException(string message) : base(message) { }
        public StreamClosedException(string message, Exception inner) : base(message, inner) { }
    }
}
