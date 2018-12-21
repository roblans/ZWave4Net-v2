using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    /// <summary>
    /// Represents the status of Neighbor Update request
    /// </summary>
    public enum NeighborUpdateStatus
    {
        Started = 0x21,
        Done = 0x22,
        Failed = 0x23,
    }
}
