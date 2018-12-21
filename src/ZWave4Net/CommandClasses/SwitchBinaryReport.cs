using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public class SwitchBinaryReport : Report
    {
        public bool Value { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Value = reader.ReadBoolean();
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Value: {Value}";
        }
    }
}
