using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class NodeReply : IPayload
    {
        public byte ClassID { get; private set; }
        public byte CommandID { get; private set; }
        public ByteArray Payload { get; private set; }

        public void Read(PayloadReader reader)
        {
            var length = reader.ReadByte();
            ClassID = reader.ReadByte();
            CommandID = reader.ReadByte();
            Payload = new ByteArray(reader.ReadBytes(length - 2));
        }

        public void Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }
    }

}
