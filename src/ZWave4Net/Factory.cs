using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZWave4Net.CommandClasses;
using ZWave4Net.CommandClasses.Services;

namespace ZWave4Net
{
    internal static class Factory
    {
        private static readonly Type[] _commandClasseServiceTypes;

        static Factory()
        {
            _commandClasseServiceTypes = typeof(CommandClassService).Assembly.GetTypes()
                .Where(element => typeof(CommandClassService).IsAssignableFrom(element))
                .Where(element => !element.IsAbstract)
                .ToArray();
        }

        private static IEnumerable<CommandClassService> CreateCommandClasseServices(ZWaveController controller, byte nodeID, byte endpointID)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");

            foreach (var commandClasseServiceType in _commandClasseServiceTypes)
            {
                yield return (CommandClassService)Activator.CreateInstance(commandClasseServiceType, controller, nodeID, endpointID);
            }
        }

        private static ProxyGenerationOptions CreateProxyGeneratorOptions(ZWaveController controller, byte nodeID, byte endpointID)
        {
            var options = new ProxyGenerationOptions();
            foreach (var service in CreateCommandClasseServices(controller, nodeID, endpointID))
            {
                options.AddMixinInstance(service);
            }
            return options;
        }

        public static Node CreateNode(ZWaveController controller, byte nodeID)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");

            var generator = new ProxyGenerator();
            var options = CreateProxyGeneratorOptions(controller, nodeID, 0);
            return (Node)generator.CreateClassProxy(typeof(Node), options, new object[] { controller, nodeID });
        }

        public static Endpoint CreateEndpoint(ZWaveController controller, byte nodeID, byte endpointID)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");
            if (endpointID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "endpointID must be greater than 0");

            var generator = new ProxyGenerator();
            var options = CreateProxyGeneratorOptions(controller, nodeID, endpointID);
            return (Endpoint)generator.CreateClassProxy(typeof(Endpoint), options, new object[] { controller, nodeID, endpointID});
        }
    }
}
