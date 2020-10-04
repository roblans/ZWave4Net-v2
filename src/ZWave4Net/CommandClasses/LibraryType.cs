using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.CommandClasses
{
    public enum LibraryType : byte
    {
        NotApplicable = 0x00,
        StaticController = 0x01,
        Controller = 0x02,
        EnhancedSlave = 0x03,
        Slave = 0x04,
        Installer = 0x05,
        RoutingSlave = 0x06,
        BridgeController = 0x07,
        DeviceUnderTest = 0x08,
        AVRemote = 0x0A,
        AVDevice = 0x0B,
    }
}
