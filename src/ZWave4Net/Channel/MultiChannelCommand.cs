using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    internal class MultiChannelCommand : Command, IEncapsulatedCommand
    {
        const byte MultiChannelEncapCommandID = 0x0D;

        public byte SourceEndpointID { get; private set; }
        public byte TargetEndpointID { get; private set; }

        public MultiChannelCommand()
        {
        }

        private MultiChannelCommand(byte sourceEndpointID, byte targetEndpointID, Payload payload)
            : base(Convert.ToByte(CommandClass.MultiChannel), MultiChannelEncapCommandID, payload)
        {
            SourceEndpointID = sourceEndpointID;
            TargetEndpointID = targetEndpointID;
            Payload = payload;
        }

        public static MultiChannelCommand Encapsulate(byte sourceEndpointID, byte targetEndpointID, Command command)
        {
            var payload = new Payload(command.Serialize().ToArray().Skip(1).ToArray());
            return new MultiChannelCommand(sourceEndpointID, targetEndpointID, payload);
        }

        Command IEncapsulatedCommand.Decapsulate()
        {
            var payload = new Payload(new[] { (byte)(Payload.Length + 2) }.Concat(Payload.ToArray()));
            return Deserialize(payload);
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
