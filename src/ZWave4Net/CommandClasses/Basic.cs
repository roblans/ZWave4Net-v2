﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using System.Reactive.Linq;

namespace ZWave4Net.CommandClasses
{
    public class Basic : CommandClassBase, IBasic
    {
        enum Command : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public Basic(Node node) : base(node, CommandClass.Basic)
        {
        }

        public async Task<BasicReport> GetValue()
        {
            var command = new NodeCommand(Class, Command.Get);
            return await Send<BasicReport>(command, Command.Report);
        }

        public Task SetValue(byte value)
        {
            var command = new NodeCommand(Class, Command.Set, value);
            return Send(command); 
        }

        public IObservable<BasicReport> Reports
        {
            get { return Receive<BasicReport>(Command.Report); }
        }
    }
}
