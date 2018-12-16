using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.CommandClasses;

namespace ZWave4Net.Channel
{
    public class EncapsulatedCommand : Command
    {
        public byte SourceEndpointID { get; private set; }
        public byte TargetEndpointID { get; private set; }
        public Command Command { get; private set; }

        public EncapsulatedCommand()
        {
            ClassID = Convert.ToByte(CommandClass.MultiChannel);
            CommandID = 0x0D;
        }

        public EncapsulatedCommand(byte sourceEndpointID, byte targetEndpointID, Command command)
            : this()
        {
            SourceEndpointID = sourceEndpointID;
            TargetEndpointID = targetEndpointID;
            Payload = new Payload(command.Serialize().ToArray().Skip(1).ToArray());
            Command = command;
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
            Command = new Command(Payload[0], Payload[1], Payload.Skip(2).ToArray());
        }

        protected override void Write(PayloadWriter writer)
        {
            writer.WriteByte((byte)(6 + Command.Payload.Length));
            writer.WriteByte(ClassID);
            writer.WriteByte(CommandID);
            writer.WriteByte(SourceEndpointID);
            writer.WriteByte(TargetEndpointID);
            writer.WriteByte(Command.ClassID);
            writer.WriteByte(Command.CommandID);
            writer.WritePayload(Command.Payload);
        }
    }
}
