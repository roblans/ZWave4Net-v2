using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public class ConfigurationReport : Report
    {
        public byte Parameter { get; private set; }
        public object Value { get; private set; }
        public byte Size { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            Parameter = reader.ReadByte();
            Size = (byte)(reader.ReadByte() & 0x07);

            switch(Size)
            {
                case 1:
                    Value = reader.ReadByte();
                    break;
                case 2:
                    Value = reader.ReadInt16();
                    break;
                case 4:
                    Value = reader.ReadInt32();
                    break;
                default:
                    throw new NotSupportedException($"Size: {Size} is not supported");
            }
        }

        public override string ToString()
        {
            return $"Parameter: {Parameter}, Value: {Value}, Size: {Size}";
        }
    }
}
