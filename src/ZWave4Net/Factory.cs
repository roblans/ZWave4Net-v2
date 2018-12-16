using Castle.DynamicProxy;
using System;
using System.Linq;
using ZWave4Net.CommandClasses;

namespace ZWave4Net
{
    public static class Factory
    {
        private static CommandClassBase[] CreateCommandClasses(ZWaveController controller, Address address)
        {
            return new CommandClassBase[]
            {
                new Basic(controller, address),
                new BinarySwitch(controller, address),
                new Association(controller, address),
            };
        }


        public static Node CreateNode(ZWaveController controller, Address address)
        {

            var generator = new ProxyGenerator();
            var options = new ProxyGenerationOptions();
            foreach (var commandClass in CreateCommandClasses(controller, address))
            {
                options.AddMixinInstance(commandClass);
            }

            return (Node)generator.CreateClassProxy(typeof(Node), options, new object[] { address });
        }

        public static Endpoint CreateEndpoint(ZWaveController controller, Address address)
        {
            var generator = new ProxyGenerator();

            var options = new ProxyGenerationOptions();
            foreach (var commandClass in CreateCommandClasses(controller, address))
            {
                options.AddMixinInstance(commandClass);
            }

            return (Endpoint)generator.CreateClassProxy(typeof(Endpoint), options, new object[] { controller, address });
        }
    }
}
