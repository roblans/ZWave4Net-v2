using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Tests
{
    [TestClass]
    public class MessageBrokerTests
    {
        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task MessageBroker_SendMessage_No_ACK_ShouldTimeout()
        {
            var cancelation = new CancellationTokenSource();

            var stream = new MockDuplexStream();
            var broker = new MessageBroker(stream);

            broker.Run(cancelation.Token);

            await broker.Send(new RequestMessage(ControllerFunction.ApplicationUpdate, new byte[] { 1, 2, 3 }), cancelation.Token);

            cancelation.Cancel();

            await broker;
        }

        [TestMethod]
        public async Task MessageBroker_SendMessage_With_ACK_ShouldSucceed()
        {
            var cancelation = new CancellationTokenSource();

            var stream = new MockDuplexStream();
            stream.AfterWrite += (s, e) =>
            {
                stream.Input.WriteByte((byte)FrameHeader.ACK);
                stream.Input.Position = 0;
            };

            var broker = new MessageBroker(stream);
            broker.Run(cancelation.Token);

            await broker.Send(new RequestMessage(ControllerFunction.ApplicationUpdate, new byte[] { 1, 2, 3 }), cancelation.Token);

            cancelation.Cancel();

            await broker;
        }

        [TestMethod]
        [ExpectedException(typeof(NakResponseException))]
        public async Task MessageBroker_SendMessage_With_NAK_Should_Throw_NakResponseException()
        {
            var cancelation = new CancellationTokenSource();

            var stream = new MockDuplexStream();
            stream.AfterWrite += (s, e) =>
            {
                stream.Input.WriteByte((byte)FrameHeader.NAK);
                stream.Input.Position--;
            };

            var broker = new MessageBroker(stream);

            broker.Run(cancelation.Token);

            await broker.Send(new RequestMessage(ControllerFunction.ApplicationUpdate, new byte[] { 1, 2, 3 }), cancelation.Token);

            cancelation.Cancel();

            await broker;
        }

        [TestMethod]
        [ExpectedException(typeof(CanResponseException))]
        public async Task MessageBroker_SendMessage_With_CAN_Should_Throw_CanResponseException()
        {
            var cancelation = new CancellationTokenSource();

            var stream = new MockDuplexStream();
            stream.AfterWrite += (s, e) =>
            {
                stream.Input.WriteByte((byte)FrameHeader.CAN);
                stream.Input.Position--;
            };

            var broker = new MessageBroker(stream);

            broker.Run(cancelation.Token);

            await broker.Send(new RequestMessage(ControllerFunction.ApplicationUpdate, new byte[] { 1, 2, 3 }), cancelation.Token);

            cancelation.Cancel();

            await broker;
        }

        [TestMethod]
        public async Task MessageBroker_SendMessage_With_NAK_NAK_ACK_Should_Succeed()
        {
            var cancelation = new CancellationTokenSource();

            var stream = new MockDuplexStream();
            var counter = 0;
            stream.AfterWrite += (s, e) =>
            {
                if (counter < 2)
                {
                    stream.Input.WriteByte((byte)FrameHeader.NAK);
                }
                else
                {
                    stream.Input.WriteByte((byte)FrameHeader.ACK);
                }
                stream.Input.Position--;
                counter++;
            };

            var broker = new MessageBroker(stream);

            broker.Run(cancelation.Token);

            await broker.Send(new RequestMessage(ControllerFunction.ApplicationUpdate, new byte[] { 1, 2, 3 }), cancelation.Token);

            cancelation.Cancel();

            await broker;
        }
    }
}
