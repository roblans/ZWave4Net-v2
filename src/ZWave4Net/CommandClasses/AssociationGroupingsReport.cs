using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public class AssociationGroupingsReport : Report
    {
        public byte SupportedGroupings  { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            SupportedGroupings  = reader.ReadByte();
        }

        public override string ToString()
        {
            return $"SupportedGroupings: {SupportedGroupings }";
        }
    }
}
