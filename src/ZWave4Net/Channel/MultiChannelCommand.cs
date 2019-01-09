using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    internal class MultiChannelCommand : EncapsulatedCommand
    {
        public const byte EncapClassID = (byte)CommandClass.MultiChannel;
        public const byte EncapCommandID = 0x0D;

        public byte SourceEndpointID { get; private set; }
        public byte TargetEndpointID { get; private set; }

        public MultiChannelCommand()
        {
        }

        public MultiChannelCommand(byte sourceEndpointID, byte targetEndpointID, Command command)
            : base(EncapClassID, EncapCommandID, command)
        {
            SourceEndpointID = sourceEndpointID;
            TargetEndpointID = targetEndpointID;
        }

        public override string ToString()
        {
            return $"{SourceEndpointID}, {TargetEndpointID}, {Payload}";
        }

        protected override void Read(PayloadReader reader)
        {
            ClassID = reader.ReadByte();
            CommandID = reader.ReadByte();
            SourceEndpointID = reader.ReadByte();
            TargetEndpointID = reader.ReadByte();
            Payload = reader.ReadObject<Payload>();
        }

        protected override void Write(PayloadWriter writer)
        {
            writer.WriteByte(ClassID);
            writer.WriteByte(CommandID);
            writer.WriteByte(SourceEndpointID);
            writer.WriteByte(TargetEndpointID);
            writer.WriteObject(Payload);
        }
    }
}
