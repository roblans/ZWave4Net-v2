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
            var bytes = new List<byte> {0, ClassID,  CommandID };
            bytes.AddRange(Payload);
            bytes[0] = (byte)(bytes.Count - 1);
            writer.WriteBytes(bytes.ToArray());
        }
    }
}
