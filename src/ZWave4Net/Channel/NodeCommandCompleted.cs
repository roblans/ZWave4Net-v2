using System;
using System.Collections.Generic;
using System.Text;
using ZWave.Channel.Protocol;

namespace ZWave.Channel
{
    internal class NodeCommandCompleted : IPayloadSerializable
    {
        public TransmissionState TransmissionState { get; private set; }
        public byte Unknown1 { get; private set; }
        public byte Unknown2 { get; private set; }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

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
