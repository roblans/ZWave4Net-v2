using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net.Channel;

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

        public async Task<byte> GetValue()
        {
            var command = new NodeCommand(Class, Command.Get);
            var response = await Send<PayloadBytes>(command, Command.Report);
            return response[0];
        }

        public Task SetValue(byte value)
        {
            var command = new NodeCommand(Class, Command.Set, value);
            return Send(command); 
        }
    }
}
