using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.CommandClasses
{
    public class FirmwareUpdateMetaDataReport : Report
    {
        public short ManufacturerID { get; private set; }
        public short FirmwareID { get; private set; }
        public short Checksum { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            ManufacturerID = reader.ReadInt16();
            FirmwareID = reader.ReadInt16();
            Checksum = reader.ReadInt16();
        }

        public override string ToString()
        {
            return $"ManufacturerID: {ManufacturerID:X4}, FirmwareID: {FirmwareID:X4}, Checksum: {Checksum:X4}";
        }
    }
}
