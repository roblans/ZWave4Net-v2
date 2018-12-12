using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public enum NodeUpdateState
    {
        InfoIncludedReceived = 0x86,
        InfoSmartStartReceived = 0x85,
        InfoReceived = 0x84,
        InfoReqDone = 0x82,
        InfoReqFailed = 0x81,
        RoutingPending = 0x80,
        NewIdAssigned = 0x40,
        DeleteDone = 0x20,
        SucId = 0x10,
    }
}
