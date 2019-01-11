namespace ZWave4Net
{
    public class UsbStick
    {
        public readonly string Name;
        public readonly ushort VendorID;
        public readonly ushort ProductID;

        public static readonly UsbStick AeotecZStick = new UsbStick("Aeotec Z-Stick", 0x0658, 0x0200);

        public UsbStick(string name, ushort vendorID, ushort productID)
        {
            Name = name;
            VendorID = vendorID;
            ProductID = productID;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
