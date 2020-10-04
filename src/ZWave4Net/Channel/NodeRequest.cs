using System;
using System.Collections.Generic;
using System.Text;
using ZWave.Channel.Protocol;

namespace ZWave.Channel
{
    internal class NodeRequest : IPayloadSerializable
    {
        public readonly byte NodeID;
        public readonly Command Command;

        public NodeRequest(byte nodeID, Command command)
        {
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");

            NodeID = nodeID;
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            throw new NotImplementedException();
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteByte(NodeID);

            var payload = Command.Serialize();
            writer.WriteByte((byte)payload.Length);
            writer.WriteObject(payload);

            writer.WriteByte((byte)(TransmitOptions.Ack | TransmitOptions.AutoRoute | TransmitOptions.Explore));
        }
    }
}
