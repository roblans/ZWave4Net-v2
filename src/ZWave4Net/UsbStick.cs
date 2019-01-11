using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public class UsbStick
    {
        public readonly string Name;
        public readonly ushort VendorId;
        public readonly ushort ProductId;

        public static readonly UsbStick AeotecZStick = new UsbStick("Aeotec Z-Stick", 0x0658, 0x0200);

        public UsbStick(string name, ushort vendorId, ushort productId)
        {
            Name = name;
            VendorId = vendorId;
            ProductId = productId;
        }
    }
}
