#if NETFRAMEWORK
using System;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.Channel
{
    public class SerialPort : ISerialPort
    {
        private readonly System.IO.Ports.SerialPort _port;

        public bool IsOpen { get; private set; } = false;

        public static string[] GetPortNames()
        {
            return System.IO.Ports.SerialPort.GetPortNames();
        }

        public SerialPort(string portName)
        {
            if (portName == null)
                throw new ArgumentNullException(nameof(portName));

            _port = new System.IO.Ports.SerialPort(portName, 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
        }

        public SerialPort(ushort vendorId, ushort productId)
        {
            var portName = FindSerialPort(vendorId, productId);
            if (portName == null)
                throw new ArgumentException("SerialPort not found");

            _port = new System.IO.Ports.SerialPort(portName, 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
        }

        private static string FindSerialPort(ushort vendorId, ushort productId)
        {
            using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM WIN32_SerialPort"))
            {
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToArray();
                foreach (var port in ports)
                {
                    var deviceID = port.GetPropertyValue("PNPDeviceID").ToString();
                    if (deviceID.Contains($"VID_{vendorId:X4}&PID_{productId:X4}"))
                    {
                        return port.GetPropertyValue("DeviceID").ToString();
                    }
                }
            }
            return null;
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

        public Task<byte[]> Read(int length, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "length cannot be less than 0");
            if (length == 0)
                return Task.FromResult(new byte[0]);

            return Task.Run(() =>
            {
                var buffer = new byte[length];
                var read = 0;

                while (read < length)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        read += _port.Read(buffer, read, Math.Min(_port.BytesToRead, length - read));
                    }
                    catch (System.IO.IOException ex)
                    {
                        throw new StreamClosedException(ex.Message, ex);
                    }
                }
                return buffer;
            }, cancellationToken);
        }

        public Task Write(byte[] values, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length == 0)
                return Task.CompletedTask;

            return Task.Run(() =>
            {
                _port.Write(values, 0, values.Length);
            }, cancellationToken);
        }
    }
}
#endif
