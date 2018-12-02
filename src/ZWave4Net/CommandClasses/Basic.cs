using System;
using System.Collections.Generic;
using System.Text;
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

        public Basic(Channel.ZWaveChannel Channel)
        {
        }

        public byte GetValue()
        {
            return 0;
        }

        public void SetValue()
        {
        }
    }
}
