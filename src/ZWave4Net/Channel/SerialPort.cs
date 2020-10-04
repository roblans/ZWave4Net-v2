using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave.Channel
{
    public class SerialPort : ISerialPort
    {
        private readonly System.IO.Ports.SerialPort _port;

        public bool IsOpen { get; private set; } = false;

        public static string[] GetPortNames()
        {
            return System.IO.Ports.SerialPort.GetPortNames();
        }

        public static string[] GetPortNames(UsbStick usbStick)
        {
            var results = new List<string>();

            using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM WIN32_SerialPort"))
            {
                var managementObjects = searcher.Get().Cast<ManagementBaseObject>().ToArray();
                foreach (var managementObject in managementObjects)
                {
                    var properties = managementObject.Properties.Cast<PropertyData>();

                    var pnpDeviceID = properties.SingleOrDefault(element => element.Name == "PNPDeviceID");
                    if (pnpDeviceID == null)
                        continue;

                    var deviceID = properties.SingleOrDefault(element => element.Name == "DeviceID");
                    if (deviceID == null)
                        continue;

                    if (pnpDeviceID.Value.ToString().Contains($"VID_{usbStick.VendorID:X4}&PID_{usbStick.ProductID:X4}"))
                    {
                        results.Add(deviceID.Value.ToString());
                    }
                }
            }
            return results.ToArray();
        }

        public SerialPort(string portName)
        {
            if (portName == null)
                throw new ArgumentNullException(nameof(portName));

            _port = new System.IO.Ports.SerialPort(portName, 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
        }

        public SerialPort(UsbStick stick)
        {
            if (stick == null)
                throw new ArgumentNullException(nameof(stick));

            var portName = GetPortNames(stick).FirstOrDefault();
            if (portName == null)
                throw new ArgumentOutOfRangeException(nameof(stick), stick, "Usb stick not found.");

            _port = new System.IO.Ports.SerialPort(portName, 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
        }

        public Task Open()
        {
            if (IsOpen)
                throw new InvalidOperationException("Port is already open");

            _port.Open();
            _port.DiscardInBuffer();
            _port.DiscardOutBuffer();

            IsOpen = true;

            return Task.CompletedTask;
        }

        public Task Close()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Port is already closed");

            _port.Close();

            IsOpen = false;

            return Task.CompletedTask;
        }

        public async Task<byte[]> Read(int length, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "length cannot be less than 0");
            if (length == 0)
                return new byte[0];

            var buffer = new byte[length];
            var read = 0;
            while (read < length)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    read += await _port.BaseStream.ReadAsync(buffer, read, length - read, cancellationToken);
                }
                catch (System.IO.IOException ex)
                {
                    throw new StreamClosedException(ex.Message, ex);
                }
            }
            return buffer;
        }

        public Task Write(byte[] values, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length == 0)
                return Task.CompletedTask;

            return _port.BaseStream.WriteAsync(values, 0, values.Length, cancellationToken);
        }
    }
}
