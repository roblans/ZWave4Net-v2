using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave
{
    public enum SpecificType
    {
        NotUsed = 0x00,

        Doorbel = GenericType.AVControlPoint << 16 | 0x12,
        SatelliteReceiver = GenericType.AVControlPoint << 16 | 0x04,
        SatelliteReceiverV2 = GenericType.AVControlPoint << 16 | 0x11,

        SimpleDisplay = GenericType.Display << 16 | 0x01,

        DoorLock = GenericType.EntryControl << 16 | 0x01,
        AdvancedDoorLock = GenericType.EntryControl << 16 | 0x02,
        SecureKeypadDoorLock = GenericType.EntryControl << 16 | 0x03,
        SecureKeypadDoorLockDeadbolt = GenericType.EntryControl << 16 | 0x04,
        SecureDoor = GenericType.EntryControl << 16 | 0x05,
        SecureGate = GenericType.EntryControl << 16 | 0x06,
        SecureBarrierAddon = GenericType.EntryControl << 16 | 0x07,
        SecureBarrierOpenOnly = GenericType.EntryControl << 16 | 0x08,
        SecureBarrierCloseOnly = GenericType.EntryControl << 16 | 0x09,
        SecureLockbox = GenericType.EntryControl << 16 | 0x0a,
        SecureKeypad = GenericType.EntryControl << 16 | 0x0b,

        PortableRemoteController = GenericType.GenericController << 16 | 0x01,
        PortableSceneController = GenericType.GenericController << 16 | 0x02,
        PortableInstallerTool = GenericType.GenericController << 16 | 0x03,
        RemoteControlAV = GenericType.GenericController << 16 | 0x04,
        RemoteControlSimple = GenericType.GenericController << 16 | 0x06,

        SimpleMeter = GenericType.Meter << 16 | 0x01,
        AdvEnergyControl = GenericType.Meter << 16 | 0x02,
        WholeHomeMeterSimple = GenericType.Meter << 16 | 0x03,

        RepeaterSlave = GenericType.RepeaterSlave << 16 | 0x01,
        VirtualNode = GenericType.RepeaterSlave << 16 | 0x02,

        ZonedSecurityPanel = GenericType.SecurityPanel << 16 | 0x01,

        AdvZensorNetAlarmSensor = GenericType.SensorAlarm << 16 | 0x05,
        AdvZensorNetSmokeSensor = GenericType.SensorAlarm << 16 | 0x0A,
        BasicRoutingAlarmSensor = GenericType.SensorAlarm << 16 | 0x01,
        BasicRoutingSmokeSensor = GenericType.SensorAlarm << 16 | 0x06,
        BasicZensorNetAlarmSensor = GenericType.SensorAlarm << 16 | 0x03,
        BasicZensorNetSmokeSensor = GenericType.SensorAlarm << 16 | 0x08,
        RoutingAlarmSensor = GenericType.SensorAlarm << 16 | 0x02,
        RoutingSmokeSensor = GenericType.SensorAlarm << 16 | 0x07,
        ZensorNetAlarmSensor = GenericType.SensorAlarm << 16 | 0x04,
        ZensorNetSmokeSensor = GenericType.SensorAlarm << 16 | 0x09,
        AlarmSensor = GenericType.SensorAlarm << 16 | 0x0B,

        RoutingSensorBinary = GenericType.SensorBinary << 16 | 0x01,

        RoutingSensorMultiLevel = GenericType.SensorMultiLevel << 16 | 0x01,
        ChimneyFan = GenericType.SensorMultiLevel << 16 | 0x02,

        PCController = GenericType.StaticController << 16 | 0x01,
        SceneController = GenericType.StaticController << 16 | 0x02,
        StaticInstallerTool = GenericType.StaticController << 16 | 0x03,
        SetTopBox = GenericType.StaticController << 16 | 0x04,
        SubSystemController = GenericType.StaticController << 16 | 0x05,
        TV = GenericType.StaticController << 16 | 0x06,
        Gateway = GenericType.StaticController << 16 | 0x07,

        PowerSwitchBinary = GenericType.SwitchBinary << 16 | 0x01,
        SceneSwitchBinary = GenericType.SwitchBinary << 16 | 0x03,
        PowerStrip = GenericType.SwitchBinary << 16 | 0x04,
        Siren = GenericType.SwitchBinary << 16 | 0x05,
        ValveOpenClose = GenericType.SwitchBinary << 16 | 0x06,
        ColorTunableBinary = GenericType.SwitchBinary << 16 | 0x02,
        UirrigationController = GenericType.SwitchBinary << 16 | 0x07,

        ClassAMotorControl = GenericType.SwitchMultiLevel << 16 | 0x05,
        ClassBMotorControl = GenericType.SwitchMultiLevel << 16 | 0x06,
        ClassCMotorControl = GenericType.SwitchMultiLevel << 16 | 0x07,
        MotorMultiPosition = GenericType.SwitchMultiLevel << 16 | 0x03,
        PowerSwitchMultilevel = GenericType.SwitchMultiLevel << 16 | 0x01,
        SceneSwitchMultilevel = GenericType.SwitchMultiLevel << 16 | 0x04,
        FanSwitch = GenericType.SwitchMultiLevel << 16 | 0x08,
        ColorTunableMultilevel = GenericType.SwitchMultiLevel << 16 | 0x02,

        SwitchRemoteBinary = GenericType.SwitchRemote << 16 | 0x01,
        SwitchRemoteMultilevel = GenericType.SwitchRemote << 16 | 0x02,
        SwitchRemoteToggleBinary = GenericType.SwitchRemote << 16 | 0x03,
        SwitchRemoteToggleMultilevel = GenericType.SwitchRemote << 16 | 0x04,

        SwitchToggleBinary = GenericType.SwitchToggle << 16 | 0x01,
        SwitchToggleMultilevel = GenericType.SwitchToggle << 16 | 0x02,

        SetbackScheduleThermostat = GenericType.Thermostat << 16 | 0x03,
        SetbackThermostat = GenericType.Thermostat << 16 | 0x05,
        SetpointThermostat = GenericType.Thermostat << 16 | 0x04,
        ThermostatGeneral = GenericType.Thermostat << 16 | 0x02,
        ThermostatGeneralV2 = GenericType.Thermostat << 16 | 0x06,
        ThermostatHeating = GenericType.Thermostat << 16 | 0x01,

        ResidentialHrv = GenericType.Ventilation << 16 | 0x01,

        SimpleWindowCovering = GenericType.WindowCovering << 16 | 0x01,

        ZipAdvNode = GenericType.ZipNode << 16 | 0x02,
        ZipTunNode = GenericType.ZipNode << 16 | 0x01,

        BasicWallController = GenericType.WallController << 16 | 0x01,

        SecureExtender = GenericType.NetworkExtender << 16 | 0x01,

        GeneralAppliance = GenericType.Appliance << 16 | 0x01,
        KitchenAppliance = GenericType.Appliance << 16 | 0x02,
        LaundryAppliance = GenericType.Appliance << 16 | 0x03,

        NotificationSensor = GenericType.SensorNotification << 16 | 0x01,
    }
}
