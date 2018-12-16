using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using System.Reactive.Linq;

namespace ZWave4Net.CommandClasses
{
    public class BinarySwitch : CommandClassBase, IBinarySwitch
    {
        enum Command : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public BinarySwitch(ZWaveController controller, Address address)
            : base(CommandClass.SwitchBinary, controller, address)
        {
        }

        public async Task<BinarySwitchReport> Get()
        {
            var command = new Channel.Command(CommandClass, Command.Get);
            return await Send<BinarySwitchReport>(command, Command.Report);
        }

        public Task Set(bool value)
        {
            var command = new Channel.Command(CommandClass, Command.Set, (byte)(value ? 0xFF : 0x00));
            return Send(command); 
        }

        public IObservable<BinarySwitchReport> Reports
        {
            get { return Reports<BinarySwitchReport>(Command.Report); }
        }
    }
}
