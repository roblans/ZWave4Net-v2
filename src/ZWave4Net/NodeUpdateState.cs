using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public enum NodeUpdateState
    {
        InfoReceived        = 0x84,
        InfoRequestDone     = 0x82,
        InfoRequestFailed   = 0x81,
        RoutingPending	    = 0x80,
        NewIDAssigned	    = 0x40,
        DeleteDone			= 0x20,
        UpdateSuccess		= 0x10,
    }
}
