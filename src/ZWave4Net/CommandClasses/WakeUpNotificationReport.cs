using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.CommandClasses
{
    /// <summary>
    /// Advertises its Wake Up destination that it is awake.
    /// </summary>
    public class WakeUpNotificationReport : Report
    {
        protected override void Read(PayloadReader reader)
        {
        }
    }
}
