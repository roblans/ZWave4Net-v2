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
        public readonly CommandClass CommandClass;
        public readonly Enum CommandID;
        public readonly byte[] Payload;

        public Command(CommandClass commandClass, Enum commandID, params byte[] payload)
        {
            CommandClass = commandClass;
            CommandID = commandID;
            Payload = payload;
        }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            throw new NotImplementedException();
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            writer.WriteByte((byte)(2 + Payload.Length));
            writer.WriteByte(Convert.ToByte(CommandClass));
            writer.WriteByte(Convert.ToByte(CommandID));
            writer.WriteBytes(Payload);
        }
    }
}
