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
            _port.Open();
            _port.DiscardInBuffer();
            _port.DiscardOutBuffer();

            return Task.CompletedTask;
        }

        public Task Close()
        {
            _port.Close();
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
                catch(System.IO.IOException ex)
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
