using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZWave4Net.Utilities;

namespace ZWave4Net.Tests
{
    [TestClass]
    public class PublisherTests
    {
        [TestMethod]
        public void PublishSubscribe()
        {
            var publisher = new Utilities.Publisher();
            var value1 = 631468;
            var value2 = "Hello world";
            var result1 = default(int);
            var result2 = default(string);

            var publisher1 = publisher.Subscibe((int value) =>
            {
                result1 = value;
            });

            var publisher2 = publisher.Subscibe((string value) =>
            {
                result2 = value;
            });

            publisher.Publish(value1);
            publisher.Publish(value2);

            Assert.AreEqual(result1, value1);
            Assert.AreEqual(result2, value2);

            publisher1.Dispose();

            result1 = default(int);
            result2 = default(string);

            publisher.Publish(value1);
            publisher.Publish(value2);

            Assert.AreEqual(result1, default(int));
            Assert.AreEqual(result2, value2);
        }
    }
}
