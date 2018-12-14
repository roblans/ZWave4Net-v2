using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class NodeRequest : IPayloadSerializable
    {
        public readonly byte NodeID;
        public readonly Command Command;

        public NodeRequest(byte nodeID, Command command)
        {
            NodeID = nodeID;
            Command = command;
        }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            throw new NotImplementedException();
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            writer.WriteByte(NodeID);
            writer.WriteObject(Command);
            writer.WriteByte((byte)(TransmitOptions.Ack | TransmitOptions.AutoRoute | TransmitOptions.Explore));
        }
    }
}
