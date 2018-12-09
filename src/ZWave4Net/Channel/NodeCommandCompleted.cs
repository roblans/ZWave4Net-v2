using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class NodeCommandCompleted : IPayloadSerializable
    {
        public TransmissionState TransmissionState { get; private set; }
        public byte Unknown1 { get; private set; }
        public byte Unknown2 { get; private set; }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            TransmissionState = (TransmissionState)reader.ReadByte();
            Unknown1 = reader.ReadByte();
            Unknown2 = reader.ReadByte();
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
