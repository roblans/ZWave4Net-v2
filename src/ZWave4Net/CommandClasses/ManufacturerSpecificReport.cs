using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public class ManufacturerSpecificReport : Report
    {
        /// <summary>
        /// https://www.silabs.com/documents/login/miscellaneous/SDS13425-Z-Wave-Plus-Assigned-Manufacturer-IDs.xlsx
        /// </summary>
        public short ManufacturerID { get; private set; }
        public short ProductType { get; private set; }
        public short ProductID { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            ManufacturerID = reader.ReadInt16();
            ProductType = reader.ReadInt16();
            ProductID = reader.ReadInt16();
        }

        public override string ToString()
        {
            return $"ManufacturerID: {ManufacturerID:X4}, ProductType: {ProductType:X4}, ProductID: {ProductID:X4}";
        }
    }
}
