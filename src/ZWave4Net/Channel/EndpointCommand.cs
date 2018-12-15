using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    public class EndpointCommand : ICommand
    {
        public readonly byte EndpointID;
        public readonly Command NodeCommand;

        public EndpointCommand(byte endpointID, Command nodeCommand)
        {
            EndpointID = endpointID;
            NodeCommand = nodeCommand;
        }

        public void Read(PayloadReader reader)
        {
            throw new NotImplementedException();
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            writer.WriteByte((byte)(6 + NodeCommand.Payload.Length));

            writer.WriteByte((byte)CommandClass.MultiChannel);
            writer.WriteByte(0x0D);
            writer.WriteByte(1);
            writer.WriteByte(EndpointID);
            writer.WriteByte(Convert.ToByte(NodeCommand.CommandClass));
            writer.WriteByte(Convert.ToByte(NodeCommand.CommandID));
            writer.WriteBytes(NodeCommand.Payload);
        }
    }
}
