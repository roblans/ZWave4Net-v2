using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    internal class MultiChannelEndcapCommand : Command, IEncapsulatedCommand
    {
        public const byte EncapClassID = (byte)CommandClass.MultiChannel;
        public const byte EncapCommandID = 0x0D;

        public byte SourceEndpointID { get; private set; }
        public byte TargetEndpointID { get; private set; }

        public MultiChannelEndcapCommand()
        {
        }

        private MultiChannelEndcapCommand(byte sourceEndpointID, byte targetEndpointID, Payload payload)
            : base(EncapClassID, EncapCommandID, payload)
        {
            SourceEndpointID = sourceEndpointID;
            TargetEndpointID = targetEndpointID;
            Payload = payload;
        }

        public static MultiChannelEndcapCommand Encapsulate(byte sourceEndpointID, byte targetEndpointID, Command command)
        {
            var payload = new Payload(command.Serialize());
            return new MultiChannelEndcapCommand(sourceEndpointID, targetEndpointID, payload);
        }

        Command IEncapsulatedCommand.Decapsulate()
        {
            return Deserialize(Payload);
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
