using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.CommandClasses
{
    /// <summary>
    /// Advertises the current Wake Up interval and destination.
    /// </summary>
    public class WakeUpIntervalReport : Report
    {
        /// <summary>
        /// The interval
        /// </summary>
        public TimeSpan Interval { get; private set; }

        /// <summary>
        /// The target node
        /// </summary>
        public byte TargetNodeID { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            Interval = TimeSpan.FromSeconds(reader.ReadInt24());
            TargetNodeID = reader.ReadByte();
        }
    }
}
