using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public class BasicReport : Report
    {
        public byte Value { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Value = reader.ReadByte();
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Value: {Value}";
        }
    }
}
