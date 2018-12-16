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
            //Logging.Factory.Subscribe((message) => WriteConsole(message));

            var port = new SerialPort(SerialPort.GetPortNames().Where(element => element != "COM1").First());
            var controller = new ZWaveController(port);

            try
            {
                await controller.Open();

                WriteInfo($"Controller Version: {controller.Version}");
                WriteInfo($"Controller ChipType: {controller.ChipType}");
                WriteInfo($"Controller HomeID: {controller.HomeID:X}");
                WriteInfo($"Controller NodeID: {controller.NodeID}");
                WriteLine();

                //var enpoint = EndpointFactory.CreateEndpoint(1, controller.Nodes[24]);
                //var basic = enpoint as IBasic;
                //await basic.Get();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                await controller.Close();
            }
        }

        public static async Task Main(string[] args)
        {
            Logging.Factory.Subscribe((message) => WriteConsole(message));

            var port = new SerialPort(SerialPort.GetPortNames().Where(element => element != "COM1").First());
            var controller = new ZWaveController(port);

            try
            {
                await controller.Open();

                //WriteInfo($"Controller Version: {controller.Version}");
                //WriteInfo($"Controller ChipType: {controller.ChipType}");
                //WriteInfo($"Controller HomeID: {controller.HomeID:X}");
                //WriteInfo($"Controller NodeID: {controller.NodeID}");
                //WriteLine();

                //foreach (var node in controller.Nodes)
                //{
                //    WriteInfo($"Node: {node}, Specific = {node.SpecificType}, Generic = {node.GenericType}, Basic = {node.BasicType}, Listening = {node.IsListening} ");

                //    if (!node.IsController && node.IsListening)
                //    {

                //        try
                //        {
                //            var nodeInfo = await node.GetNodeInfo();
                //            WriteInfo($"Node: {node}, CommandClasses = {string.Join(", ", nodeInfo.SupportedCommandClasses)}");
                //        }
                //        catch (OperationFailedException ex)
                //        {
                //            WriteError($"Error: {ex.Message}");
                //        }
                //    }

                //    var neighbours = await node.GetNeighbours();
                //    WriteInfo($"Node: {node}, Neighbours = {string.Join(", ", neighbours.Cast<object>().ToArray())}");

                //    WriteLine();
                //}

                //foreach (var basic in controller.Nodes.Where(element => !element.IsController && element.IsListening).Cast<IBasic>())
                //{
                //    var stopwatch = Stopwatch.StartNew();

                //    var report = await basic.Get();
                //    Console.WriteLine($"{report} {stopwatch.Elapsed}");

                //    basic.Reports.Subscribe((element) => WriteInfo(element));
                //}

                foreach (var basic in controller.Nodes.Where(element => !element.IsController && element.IsListening).Cast<ISwitchBinary>())
                {
                    basic.Reports.Subscribe((r) => WriteError(r));
                }

                Console.Clear();
                var switchBinary = (ISwitchBinary)controller.Nodes[25].Endpoints[1];
                await switchBinary.Set(false);
                var value = await switchBinary.Get();
                //await Task.Delay(100);
                //await Task.Delay(100);
                //await switchBinary.Set(false);

                Console.ReadLine();

                await controller.Close();

            }
            catch(Exception ex)
            {
                WriteError(ex);
                Console.ReadLine();
            }
        }

        private static void WriteConsole(LogRecord record)
        {
            switch (record.Level)
            {
                case LogLevel.Debug:
                    WriteDebug(record);
                    break;
                case LogLevel.Info:
                    WriteInfo(record);
                    break;
                case LogLevel.Warning:
                    WriteWarning(record);
                    break;
                case LogLevel.Error:
                    WriteError(record);
                    break;
            }
        }

        private static void WriteLine()
        {
            Console.WriteLine();
        }

        private static void WriteDebug(object state)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(state);
        }

        private static void WriteInfo(object state)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(state);
        }

        private static void WriteWarning(object state)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(state);
        }

        private static void WriteError(object state)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(state);
        }
    }
}
