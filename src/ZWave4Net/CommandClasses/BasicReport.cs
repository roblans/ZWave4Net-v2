using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.CommandClasses
{
    /// <summary>
    /// Advertises the status of the primary functionality of the node
    /// </summary>
    public class BasicReport : Report
    {
        /// <summary>
        /// The current value
        /// </summary>
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
