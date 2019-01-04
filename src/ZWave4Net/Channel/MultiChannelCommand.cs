using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    internal class MultiChannelCommand : Command
    {
        public byte SourceEndpointID { get; private set; }
        public byte TargetEndpointID { get; private set; }

        public MultiChannelCommand()
        {
        }

        private MultiChannelCommand(byte sourceEndpointID, byte targetEndpointID, Payload payload)
            : base(Convert.ToByte(CommandClass.MultiChannel), 0x0D, payload)
        {
            SourceEndpointID = sourceEndpointID;
            TargetEndpointID = targetEndpointID;
            Payload = payload;
        }

        public static MultiChannelCommand Wrap(byte sourceEndpointID, byte targetEndpointID, Command command)
        {
            var payload = new Payload(command.Serialize().ToArray().Skip(1).ToArray());
            return new MultiChannelCommand(sourceEndpointID, targetEndpointID, payload);
        }

        public Command Unwrap()
        {
            return new Command(Payload[0], Payload[1], Payload.Skip(2).ToArray());
        }

        public override string ToString()
        {
            return $"{SourceEndpointID}, {TargetEndpointID}, {Payload}";
        }

        protected override void Read(PayloadReader reader)
        {
            var length = reader.ReadByte();
            ClassID = reader.ReadByte();
            CommandID = reader.ReadByte();
            SourceEndpointID = reader.ReadByte();
            TargetEndpointID = reader.ReadByte();
            Payload = reader.ReadObject<Payload>();
        }

        protected override void Write(PayloadWriter writer)
        {
            writer.WriteByte((byte)(4 + Payload.Length));
            writer.WriteByte(ClassID);
            writer.WriteByte(CommandID);
            writer.WriteByte(SourceEndpointID);
            writer.WriteByte(TargetEndpointID);
            writer.WriteObject(Payload);
        }
    }
}
