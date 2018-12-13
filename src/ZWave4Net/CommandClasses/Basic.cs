using System;
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

        public Basic() : base(CommandClass.Basic)
        {
        }

        public async Task<BasicReport> Get()
        {
            var command = new NodeCommand(CommandClass, Command.Get);
            return await Send<BasicReport>(command, Command.Report);
        }

        public Task Set(byte value)
        {
            var command = new NodeCommand(CommandClass, Command.Set, value);
            return Send(command); 
        }

        public IObservable<BasicReport> Reports
        {
            get { return Reports<BasicReport>(Command.Report); }
        }
    }
}
