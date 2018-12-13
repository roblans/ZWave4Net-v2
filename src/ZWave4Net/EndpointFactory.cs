using Castle.DynamicProxy;
using System;
using System.Linq;
using ZWave4Net.CommandClasses;

namespace ZWave4Net
{
    public static class EndpointFactory
    {
        private static CommandClassBase[] CreateCommandClasses()
        {
            return new CommandClassBase[]
            {
                new Basic()
            };
        }

        private static ProxyGenerationOptions CreateProxyGenerationOptions()
        {
            var options = new ProxyGenerationOptions();
            foreach (var commandClass in CreateCommandClasses())
            {
                options.AddMixinInstance(commandClass);
            }
            return options;
        }

        public static Node CreateNode(byte nodeID, ZWaveController controller)
        {
            var generator = new ProxyGenerator();
            var options = CreateProxyGenerationOptions();

            var nodeProxy = (Node)generator.CreateClassProxy(typeof(Node), options, new object[] { nodeID, controller });

            foreach(var commandClass in options.MixinsAsArray().OfType< CommandClassBase>())
            {
                commandClass.Initialize(nodeProxy);
            }
            return nodeProxy;
        }

        public static Endpoint CreateEndpoint(byte endpointID, Node node)
        {
            var generator = new ProxyGenerator();
            var options = CreateProxyGenerationOptions();

            var endpointProxy = (Endpoint)generator.CreateClassProxy(typeof(Endpoint), options, new object[] { endpointID, node });

            foreach (var commandClass in options.MixinsAsArray().OfType<CommandClassBase>())
            {
                commandClass.Initialize(endpointProxy);
            }
            return endpointProxy;
        }
    }
}
