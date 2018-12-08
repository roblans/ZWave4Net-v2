using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class NodeRequest : IPayload
    {
        public readonly byte NodeID;
        public readonly NodeCommand Command;

        public NodeRequest(byte nodeID, NodeCommand command)
        {
            NodeID = nodeID;
            Command = command;
        }

        void IPayload.Read(PayloadReader reader)
        {
            throw new NotImplementedException();
        }

        void IPayload.Write(PayloadWriter writer)
        {
            writer.WriteByte(NodeID);
            writer.WriteObject(Command);
            writer.WriteByte((byte)(TransmitOptions.Ack | TransmitOptions.AutoRoute | TransmitOptions.Explore));
        }
    }
}
