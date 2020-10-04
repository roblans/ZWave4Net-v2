using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.CommandClasses
{
    /// <summary>
    /// Advertises the status of a device with On/Off or Enable/Disable capability.
    /// </summary>
    public class SwitchBinaryReport : Report
    {
        /// <summary>
        /// The current value
        /// </summary>
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
