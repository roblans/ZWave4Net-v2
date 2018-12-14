using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using System.Reactive.Linq;

namespace ZWave4Net.CommandClasses
{
    public class SwitchBinary : CommandClassBase, ISwitchBinary
    {
        enum Command : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public SwitchBinary(ZWaveController controller, byte nodeID, byte endpointID)
            : base(CommandClass.SwitchBinary, controller, nodeID, endpointID)
        {
        }

        public async Task<SwitchBinaryReport> Get()
        {
            var command = new Channel.Command(CommandClass, Command.Get);
            return await Send<SwitchBinaryReport>(command, Command.Report);
        }

        public Task Set(bool value)
        {
            var command = new Channel.Command(CommandClass, Command.Set, (byte)(value ? 0xFF : 0x00));
            return Send(command); 
        }

        public IObservable<SwitchBinaryReport> Reports
        {
            get { return Reports<SwitchBinaryReport>(Command.Report); }
        }
    }
}
