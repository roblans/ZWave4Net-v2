using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class EndpointReply : IPayloadSerializable
    {
        public byte ClassID { get; private set; }
        public byte CommandID { get; private set; }
        public Payload Payload { get; private set; }

        public void Read(PayloadReader reader)
        {
            var length = reader.ReadByte();
            ClassID = reader.ReadByte();
            CommandID = reader.ReadByte();
            Payload = new Payload(reader.ReadBytes(length - 2));
        }

        public void Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }
    }

}
