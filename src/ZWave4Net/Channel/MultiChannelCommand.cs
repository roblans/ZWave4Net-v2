using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave.CommandClasses;

namespace ZWave.Channel
{
    internal class MultiChannelCommand : Command, IEncapsulatedCommand
    {
        public const byte EncapCommandID = 0x0D;

        public byte SourceEndpointID { get; private set; }
        public byte TargetEndpointID { get; private set; }

        private MultiChannelCommand(byte sourceEndpointID, byte targetEndpointID, Payload payload)
            : base(CommandClass.MultiChannel, EncapCommandID, payload)
        {
            SourceEndpointID = sourceEndpointID;
            TargetEndpointID = targetEndpointID;
        }

        public MultiChannelCommand()
        {
        }

        public static MultiChannelCommand Encapsulate(byte sourceEndpointID, byte targetEndpointID, Command command)
        {
            var payload = command.Serialize();
            return new MultiChannelCommand(sourceEndpointID, targetEndpointID, payload);
        }

        public Command Decapsulate()
        {
            return Command.Parse(Payload);
        }

        public override string ToString()
        {
            return $"{SourceEndpointID}, {TargetEndpointID}, {Payload}";
        }

        protected override void Read(PayloadReader reader)
        {
            CommandClass = (CommandClass)reader.ReadByte();
            CommandID = reader.ReadByte();
            SourceEndpointID = reader.ReadByte();
            TargetEndpointID = reader.ReadByte();
            Payload = reader.ReadObject<Payload>();
        }

        protected override void Write(PayloadWriter writer)
        {
            writer.WriteByte((byte)CommandClass);
            writer.WriteByte(CommandID);
            writer.WriteByte(SourceEndpointID);
            writer.WriteByte(TargetEndpointID);
            writer.WriteObject(Payload);
        }
    }
}
