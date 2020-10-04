using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ZWave.Channel
{
    public static partial class Extentions
    {
        public static short CalculateCrc16Checksum(this IEnumerable<byte> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            // SDS12657-12-Z-Wave-Command-Class-Specification-A-M.pdf | 4.41.1 CRC-16 Encapsulated Command
            // The checksum algorithm implements a CRCCCITT using initialization value equal to 0x1D0F and 0x1021 (normal representation) as the poly
            var crc = 0x1D0F;
            var polynomial = 0x1021;

            foreach (var value in values)
            {
                for (var i = 0; i < 8; i++)
                {
                    var bit = ((value >> (7 - i) & 1) == 1);
                    var c15 = ((crc >> 15 & 1) == 1);
                    crc <<= 1;
                    if (c15 ^ bit)
                    {
                        crc ^= polynomial;
                    }
                }
            }
            crc &= 0xffff;
            return (short)crc;
        }
    }
}
