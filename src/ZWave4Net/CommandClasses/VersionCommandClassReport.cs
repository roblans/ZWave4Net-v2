using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public class VersionCommandClassReport : Report
    {
        public CommandClass CommandClass { get; private set; }
        public byte Version { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            var commandClass = reader.ReadByte();
            CommandClass = (CommandClass)commandClass;
            Version = reader.ReadByte();
        }

        public override string ToString()
        {
            return $"CommandClass: {CommandClass}, Version: {Version}";
        }
    }
}
