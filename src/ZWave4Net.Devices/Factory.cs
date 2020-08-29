using Castle.DynamicProxy;
using System;

namespace ZWave4Net.Devices
{
    public static class Factory
    {
        public static T CreateDevice<T>(Node node) where T : IDevice
        {
            var generator = new ProxyGenerator();
            return (T)generator.CreateClassProxy(typeof(Device), new[] { typeof(T) }, new ProxyGenerationOptions(), new[] { node });
        }
    }
}
