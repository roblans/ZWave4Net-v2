﻿using System;
using System.Collections.Generic;
using System.Text;
using ZWave.Channel.Protocol;

namespace ZWave.Channel
{
    internal class NodeResponse : IPayloadSerializable
    {
        public ReceiveStatus Status { get; private set; }
        public byte NodeID { get; private set; }
        public Command Command { get; private set; }

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var status = reader.ReadByte();

            Status = ReceiveStatus.None;
            if ((status & 0x01) > 0)
                Status |= ReceiveStatus.RoutedBusy;
            if ((status & 0x02) > 0)
                Status |= ReceiveStatus.LowPower;
            if ((status & 0x0C) == 0x00)
                Status |= ReceiveStatus.TypeSingle;
            if ((status & 0x0C) == 0x01)
                Status |= ReceiveStatus.TypeBroad;
            if ((status & 0x0C) == 0x10)
                Status |= ReceiveStatus.TypeMulti;
            if ((status & 0x10) > 0)
                Status |= ReceiveStatus.TypeExplore;
            if ((status & 0x40) > 0)
                Status |= ReceiveStatus.ForeignFrame;

            NodeID = reader.ReadByte();

            var commandLength = reader.ReadByte();
            var commandBytes = reader.ReadBytes(commandLength);
            Command = Command.Parse(new Payload(commandBytes));
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
