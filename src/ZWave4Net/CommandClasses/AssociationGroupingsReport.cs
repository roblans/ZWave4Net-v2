using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.CommandClasses
{
    public class AssociationGroupingsReport : Report
    {
        public byte SupportedGroupings  { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            SupportedGroupings = reader.ReadByte();
        }

        public override string ToString()
        {
            return $"SupportedGroupings: {SupportedGroupings }";
        }
    }
}
