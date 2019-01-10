using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public enum CommandClass : byte
    {
        NoOperation = 0x00,
        Basic = 0x20,
        SwitchBinary = 0x25,
        SwitchMultiLevel = 0x26,
        SwitchAll = 0x27,
        SceneActivation = 0x2B,
        SensorBinary = 0x30,
        SensorMultiLevel = 0x31,
        Meter = 0x32,
        Color = 0x33,
        ThermostatSetpoint = 0x43,
        CentralScene = 0x5B,
        Crc16Encap = 0x56,
        MultiChannel = 0x60,
        Configuration = 0x70,
        Alarm = 0x71,
        ManufacturerSpecific = 0x72,
        Powerlevel = 0x73,
        Protection = 0x75,
        FirmwareUpdateMetaData = 0x7A,
        Battery = 0x80,
        Clock = 0x81,
        WakeUp = 0x84,
        Association = 0x85,
        Version = 0x86,
        MultiChannelAssociation = 0x8E,
        SensorAlarm = 0x9C,
    }
}
