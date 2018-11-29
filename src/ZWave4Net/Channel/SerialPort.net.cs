using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.Channel
{
#if NET47
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

        public async Task<byte[]> Read(int lenght, CancellationToken cancellation)
        {
            var buffer = new byte[lenght];

            var read = 0;
            while (read < lenght)
            {
                try
                {
                    read += await _port.BaseStream.ReadAsync(buffer, read, lenght - read, cancellation);
                }
                catch (System.IO.IOException ex)
                {
                    throw new StreamClosedException(ex.Message, ex);
                }
            }

            return buffer;
        }

        public Task Write(byte[] values, CancellationToken cancellation)
        {
            return _port.BaseStream.WriteAsync(values, 0, values.Length, cancellation);
        }
    }
#endif
}
