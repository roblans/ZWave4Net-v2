using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class NodeCommand : IPayload
    {
        public readonly byte ClassID;
        public readonly byte CommandID;
        public readonly byte[] Payload;

        public NodeCommand(byte classID, byte commandID, params byte[] payload)
        {
            ClassID = classID;
            CommandID = commandID;
            Payload = payload;
        }

        void IPayload.Read(PayloadReader reader)
        {
            throw new NotImplementedException();
        }

        void IPayload.Write(PayloadWriter writer)
        {
            writer.WriteByte((byte)(2 + Payload.Length));
            writer.WriteByte(ClassID);
            writer.WriteByte(CommandID);
            writer.WriteBytes(Payload);
        }
    }
}
