using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.CommandClasses;
using ZWave4Net.Diagnostics;
using System.Reactive.Linq;
using ZWave4Net.Channel.Protocol.Frames;

namespace ChannelConsole
{
    class Program
    {
        public static async Task Main2(string[] args)
        {
            Logging.Factory.Subscribe((message) => WriteConsole(message));

            var cancellation = new CancellationTokenSource();

            var port = new SerialPort(SerialPort.GetPortNames().Where(element => element != "COM1").First());
            await port.Open();

            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Starting");

            var broker = new MessageBroker(port);

            //var subsciption1 = broker.Subscribe((frame) =>
            //{
            //    Console.ForegroundColor = ConsoleColor.Yellow;
            //    Console.WriteLine($"Next {frame}");
            //},
            //() =>
            //{
            //    Console.ForegroundColor = ConsoleColor.Yellow;
            //    Console.WriteLine($"Complete");
            //});

            //var subsciption2 = broker.OfType<DataFrame>().Subscribe((frame) =>
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    Console.WriteLine($"Next {frame}");
            //},
            //() =>
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    Console.WriteLine($"Complete");
            //});

            //broker.Run(cancellation.Token);


            Console.ReadLine();

            //subsciption1.Dispose();
            //subsciption2.Dispose();

            await broker;

            Console.ReadLine();

            cancellation.Cancel();

            //await broker;

            await port.Close();
        }

        public static async Task Main(string[] args)
        {
            Logging.Factory.Subscribe((message) => WriteConsole(message));

            var port = new SerialPort(SerialPort.GetPortNames().Where(element => element != "COM1").First());
            var controller = new ZWaveController(port);

            try
            {
                await controller.Open();

                Console.WriteLine($"Controller Version: {controller.Version}");
                Console.WriteLine($"Controller ChipType: {controller.ChipType}");
                Console.WriteLine($"Controller HomeID: {controller.HomeID:X}");
                Console.WriteLine($"Controller NodeID: {controller.NodeID}");
                //Console.WriteLine();

                foreach (var node in controller.Nodes)
                {
                    var protocolInfo = await node.GetProtocolInfo();
                    Console.WriteLine($"Node: {node}, Specific = {protocolInfo.SpecificType}, Generic = {protocolInfo.GenericType}, Basic = {protocolInfo.BasicType}, Listening = {protocolInfo.IsListening} ");

                    var neighbours = await node.GetNeighbours();
                    Console.WriteLine($"Node: {node}, Neighbours = {string.Join(", ", neighbours.Cast<object>().ToArray())}");

                    Console.WriteLine();
                }

                var powerSwitch = controller.Nodes[24];
                await powerSwitch.RequestNeighborUpdate(new Progress<NeighborUpdateStatus>(status =>
                {
                    Console.WriteLine($"RequestNeighborUpdate: {status}");
                }));

                var basic = new Basic(powerSwitch);
                await basic.SetValue(0);
                var value = await basic.GetValue();

                Console.WriteLine(value);

                Console.ReadLine();

                await controller.Close();

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
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
