using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    public interface ICommand : IPayloadSerializable
    {

    }

    public class Command : ICommand
    {
        public readonly byte ClassID;
        public readonly byte CommandID;
        public readonly byte[] Payload;

        public Command(CommandClass @class, Enum command, params byte[] payload)
        {
            ClassID = Convert.ToByte(@class);
            CommandID = Convert.ToByte(command);
            Payload = payload;
        }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            throw new NotImplementedException();
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            writer.WriteByte((byte)(2 + Payload.Length));
            writer.WriteByte(ClassID);
            writer.WriteByte(CommandID);
            writer.WriteBytes(Payload);
        }
    }
}
