﻿using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    public class NodeCommand : IPayload
    {
        public readonly CommandClass Class;
        public readonly Enum Command;
        public readonly byte[] Payload;

        public NodeCommand(CommandClass @class, Enum command, params byte[] payload)
        {
            Class = @class;
            Command = command;
            Payload = payload;
        }


        void IPayload.Read(PayloadReader reader)
        {
            throw new NotImplementedException();
        }

        void IPayload.Write(PayloadWriter writer)
        {
            writer.WriteByte((byte)(2 + Payload.Length));
            writer.WriteByte(Convert.ToByte(Class));
            writer.WriteByte(Convert.ToByte(Command));
            writer.WriteBytes(Payload);
        }
    }
}
