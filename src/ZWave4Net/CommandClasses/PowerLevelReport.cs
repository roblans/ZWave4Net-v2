using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public class PowerlevelReport : Report
    {
        public Powerlevel Level { get; private set; }
        public TimeSpan Timeout { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            Level = (Powerlevel)reader.ReadByte();
            Timeout = TimeSpan.FromSeconds(reader.ReadByte());
        }

        public override string ToString()
        {
            return $"Level: {Level}, Timeout: {Timeout}";
        }

    }
}
