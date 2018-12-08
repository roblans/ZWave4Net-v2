﻿using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class NodeResponse : IPayload
    {
        public ReceiveStatus Status { get; private set; }
        public byte NodeID { get; private set; }
        public Payload Payload { get; private set; }

        void IPayload.Read(PayloadReader reader)
        {
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
            Payload = reader.ReadObject<Payload>();
        }

        void IPayload.Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
