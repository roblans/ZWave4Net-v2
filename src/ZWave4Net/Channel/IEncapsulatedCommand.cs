using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel
{
    internal interface IEncapsulatedCommand
    {
        Command Decapsulate();

        //public EncapsulatedCommand()
        //{
        //}

        //protected EncapsulatedCommand(byte classID, byte commandID, Command command)
        //    : base(classID, commandID)
        //{
        //    Payload = new Payload(command.Serialize());
        //}

        //public Command Decapsulate()
        //{
        //    return CommandFactory.CreateCommand(Payload);
        //}
    }
}
