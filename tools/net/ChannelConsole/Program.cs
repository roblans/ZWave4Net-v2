﻿using System;
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
        public static async Task Main(string[] args)
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

                foreach (var node in controller.Nodes)
                {
                    var protocolInfo = await node.GetProtocolInfo();
                    WriteInfo($"Node: {node}, Specific = {protocolInfo.SpecificType}, Generic = {protocolInfo.GenericType}, Basic = {protocolInfo.BasicType}, Listening = {protocolInfo.IsListening} ");

                    if (node.NodeID != controller.NodeID && protocolInfo.IsListening)
                    {

                        try
                        {
                            var nodeInfo = await node.GetNodeInfo();
                            WriteInfo($"Node: {node}, CommandClasses = {string.Join(", ", nodeInfo.SupportedCommandClasses)}");
                        }
                        catch (OperationFailedException ex)
                        {
                            WriteError($"Error: {ex.Message}");
                        }
                    }

                    var neighbours = await node.GetNeighbours();
                    WriteInfo($"Node: {node}, Neighbours = {string.Join(", ", neighbours.Cast<object>().ToArray())}");

                    WriteLine();
                }

                var powerSwitch = controller.Nodes[24];

                await powerSwitch.RequestNeighborUpdate(new Progress<NeighborUpdateStatus>(status =>
                {
                    WriteInfo($"RequestNeighborUpdate: {status}");
                }));

                using (powerSwitch.Updates.Subscribe(async (element) => 
                {
                    WriteInfo(element);
                    var basic2 = new Basic(powerSwitch);
                    WriteInfo(await basic2.Get());
                }))
                {
                    Console.ReadLine();
                }

                Console.ReadLine();

                var basic = new Basic(powerSwitch);
                await basic.Set(0);
                var value = await basic.Get();

                WriteInfo(value);

                using (basic.Reports.Subscribe((element) => WriteInfo(element)))
                {
                    await basic.Set(255);
                    await Task.Delay(1000);
                    await basic.Set(0);
                    Console.ReadLine();
                }

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
