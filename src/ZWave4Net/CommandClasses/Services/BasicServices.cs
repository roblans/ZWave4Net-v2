using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using System.Reactive.Linq;

namespace ZWave4Net.CommandClasses.Services
{
    public class BasicServices : CommandClassService, IBasic
    {
        enum Command : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public BasicServices(ZWaveController controller, byte nodeID, byte endpointID)
            : base(CommandClass.Basic, controller, nodeID, endpointID)
        {
        }

        public Task<BasicReport> Get()
        {
            var command = new Channel.Command(CommandClass, Command.Get);
            return Send<BasicReport>(command, Command.Report);
        }

        public Task Set(byte value)
        {
            var command = new Channel.Command(CommandClass, Command.Set, value);
            return Send(command); 
        }

        public IObservable<BasicReport> Reports
        {
            get { return Reports<BasicReport>(Command.Report); }
        }
    }
}
