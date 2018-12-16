using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZWave4Net.CommandClasses;
using ZWave4Net.CommandClasses.Services;

namespace ZWave4Net
{
    public static class Factory
    {
        private static readonly Type[] _commandClasseServiceTypes;

        static Factory()
        {
            _commandClasseServiceTypes = typeof(CommandClassService).Assembly.GetTypes()
                .Where(element => typeof(CommandClassService).IsAssignableFrom(element))
                .Where(element => element != typeof(CommandClassService))
                .ToArray();
        }

        private static IEnumerable<CommandClassService> CreateCommandClasseServices(ZWaveController controller, byte nodeID, byte endpointID)
        {
            foreach(var commandClasseServiceType in _commandClasseServiceTypes)
            {
                yield return (CommandClassService)Activator.CreateInstance(commandClasseServiceType, controller, nodeID, endpointID);
            }
        }


        public static Node CreateNode(ZWaveController controller, byte nodeID)
        {

            var generator = new ProxyGenerator();
            var options = new ProxyGenerationOptions();
            foreach (var service in CreateCommandClasseServices(controller, nodeID, 0))
            {
                options.AddMixinInstance(service);
            }

            return (Node)generator.CreateClassProxy(typeof(Node), options, new object[] { controller, nodeID });
        }

        public static Endpoint CreateEndpoint(ZWaveController controller, byte nodeID, byte endpointID)
        {
            var generator = new ProxyGenerator();

            var options = new ProxyGenerationOptions();
            foreach (var service in CreateCommandClasseServices(controller, nodeID, endpointID))
            {
                options.AddMixinInstance(service);
            }

            return (Endpoint)generator.CreateClassProxy(typeof(Endpoint), options, new object[] { controller, nodeID, endpointID});
        }
    }
}
