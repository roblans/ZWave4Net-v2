using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Utilities;

namespace ChannelConsole
{
    class Program
    {
        public static async Task Main(string[] args)
        {

            Logging.Subscribe((message) => Console.WriteLine(message));

            var port = new SerialPort(SerialPort.GetPortNames().Where(element => element != "COM1").First());
            var channel = new ZWaveChannel(port);

            await channel.Open();

            Console.ReadLine();

            await channel.Close();

            Console.ReadLine();
        }
    }
}
