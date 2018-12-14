using Castle.DynamicProxy;
using System;
using System.Linq;
using ZWave4Net.CommandClasses;

namespace ZWave4Net
{
    public static class EndpointFactory
    {
        private static CommandClassBase[] CreateCommandClasses(ZWaveController controller, byte nodeID, byte endpointID)
        {
            return new CommandClassBase[]
            {
                new Basic(controller, nodeID, endpointID),
            };
        }


        public static Node CreateNode(ZWaveController controller, byte nodeID)
        {
            var generator = new ProxyGenerator();

            var options = new ProxyGenerationOptions();
            foreach (var commandClass in CreateCommandClasses(controller, nodeID, 0))
            {
                options.AddMixinInstance(commandClass);
            }

            return (Node)generator.CreateClassProxy(typeof(Node), options, new object[] { controller, nodeID });
        }

        public static Endpoint CreateEndpoint(ZWaveController controller, byte nodeID, byte endpointID)
        {
            var generator = new ProxyGenerator();

            var options = new ProxyGenerationOptions();
            foreach (var commandClass in CreateCommandClasses(controller, nodeID, endpointID))
            {
                options.AddMixinInstance(commandClass);
            }

            return (Endpoint)generator.CreateClassProxy(typeof(Endpoint), options, new object[] { controller, nodeID, endpointID });
        }
    }
}
