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

        private static IEnumerable<CommandClassService> CreateCommandClasseServices(byte nodeID, byte endpointID, ZWaveController controller)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");

            foreach (var commandClasseServiceType in _commandClasseServiceTypes)
            {
                yield return (CommandClassService)Activator.CreateInstance(commandClasseServiceType, nodeID, endpointID, controller);
            }
        }

        private static ProxyGenerationOptions CreateProxyGeneratorOptions(byte nodeID, byte endpointID, ZWaveController controller)
        {
            var options = new ProxyGenerationOptions();
            foreach (var service in CreateCommandClasseServices(nodeID, endpointID, controller))
            {
                options.AddMixinInstance(service);
            }
            return options;
        }

        public static Node CreateNode(byte nodeID, ZWaveController controller)
        {
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            var generator = new ProxyGenerator();
            var options = CreateProxyGeneratorOptions(nodeID, 0, controller);
            return (Node)generator.CreateClassProxy(typeof(Node), options, new object[] { nodeID, controller });
        }

        public static Endpoint CreateEndpoint(byte nodeID, byte endpointID, ZWaveController controller)
        {
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");
            if (endpointID == 0)
                throw new ArgumentOutOfRangeException(nameof(endpointID), endpointID, "endpointID must be greater than 0");
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            var generator = new ProxyGenerator();
            var options = CreateProxyGeneratorOptions(nodeID, endpointID, controller);
            return (Endpoint)generator.CreateClassProxy(typeof(Endpoint), options, new object[] { nodeID, endpointID, controller});
        }
    }
}
