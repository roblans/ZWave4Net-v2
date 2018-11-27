using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.Diagnostics;

namespace ChannelConsole
{
    class Program
    {
        public static async Task Main(string[] args)
        {

            Logging.Factory.Subscribe((message) => WriteConsole(message));

            var port = new SerialPort(SerialPort.GetPortNames().Where(element => element != "COM1").First());
            var channel = new ZWaveChannel(port);

            await channel.Open();

            var result = await channel.Send(new RequestMessage(Function.MemoryGetId, new byte[0]), default);
            await Task.Delay(5000);

            Console.ReadLine();

            await channel.Close();

            Console.ReadLine();
        }

        private static void WriteConsole(LogRecord record)
        {
            switch (record.Level)
            {
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            Console.WriteLine(record);
        }
    }
}
