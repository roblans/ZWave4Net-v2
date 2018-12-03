using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net.Channel;

namespace ZWave4Net.CommandClasses
{
    public class Basic : CommandClass, IBasic
    {
        enum Command : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public Basic(Node node) : base(node)
        {
        }

        public async Task<byte> GetValue()
        {
            var command = new NodeCommand(0x20, 0x02);
            var payload = await Send<Payload>(command);
            return 123;
        }

        public Task SetValue(byte value)
        {
            var command = new NodeCommand(0x20, 0x01, value);
            return Send(command); 
        }
    }
}
