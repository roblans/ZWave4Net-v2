using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.CommandClasses;

namespace ZWave4Net
{
    public static class EndpointFactory
    {
        public static IEndpoint CreateEndpoint(byte endpointID, Node node)
        {
            var endpoint = new Endpoint(endpointID, node);
            var proxyGenerator = new ProxyGenerator();
            return (IEndpoint)proxyGenerator.CreateInterfaceProxyWithTargetInterface(typeof(IEndpoint), new[] { typeof(IBasic) }, endpoint, new EndpointInterceptor(endpoint));
        }

        class EndpointInterceptor : IInterceptor
        {
            public readonly Endpoint Endpoint;

            public EndpointInterceptor(Endpoint endpoint)
            {
                Endpoint = endpoint;
            }

            public void Intercept(IInvocation invocation)
            {
                if (typeof(IBasic).IsAssignableFrom(invocation.Method.DeclaringType))
                {
                    var changeProxyTarget = invocation as IChangeProxyTarget;
                    var target = Endpoint.GetCommandClass(typeof(IBasic));
                    changeProxyTarget.ChangeInvocationTarget(target);
                    invocation.Proceed();
                }
            }
        }
    }
}
