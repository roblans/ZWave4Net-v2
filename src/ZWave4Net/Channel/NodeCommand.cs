using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    public class NodeCommand : IPayloadSerializable
    {
        public TimeSpan ReplyTimeout = TimeSpan.FromSeconds(1);
        public int MaxRetryAttempts = 3;

        public readonly CommandClass Class;
        public readonly Enum Command;
        public readonly byte[] Payload;

        public NodeCommand(CommandClass @class, Enum command, params byte[] payload)
        {
            Class = @class;
            Command = command;
            Payload = payload;
        }


        void IPayloadSerializable.Read(PayloadReader reader)
        {
            throw new NotImplementedException();
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            writer.WriteByte((byte)(2 + Payload.Length));
            writer.WriteByte(Convert.ToByte(Class));
            writer.WriteByte(Convert.ToByte(Command));
            writer.WriteBytes(Payload);
        }
    }
}
