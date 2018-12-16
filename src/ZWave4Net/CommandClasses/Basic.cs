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
        enum command : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public Basic(ZWaveController controller, byte nodeID, byte endpointID)
            : base(CommandClass.Basic, controller, nodeID, endpointID)
        {
        }

        public Task<BasicReport> Get()
        {
            var command = new Command(CommandClass, Basic.command.Get);
            return Send<BasicReport>(command, Basic.command.Report);
        }

        public Task Set(byte value)
        {
            var command = new Command(CommandClass, Basic.command.Set, value);
            return Send(command); 
        }

        public IObservable<BasicReport> Reports
        {
            get { return Reports<BasicReport>(command.Report); }
        }
    }
}
