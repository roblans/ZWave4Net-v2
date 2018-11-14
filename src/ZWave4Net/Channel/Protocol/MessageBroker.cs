using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.Channel.Protocol
{
    public class MessageBroker
    {
        private readonly ConcurrentDictionary<Delegate, object> _subscribers = new ConcurrentDictionary<Delegate, object>();
        private FrameReader _reader;
        private FrameWriter _writer;
        private Task _task;

        public readonly CancellationToken Cancelation;

        public MessageBroker(IByteStream stream, CancellationToken cancelation)
        {
            _reader = new FrameReader(stream);
            _writer = new FrameWriter(stream);

            Cancelation = cancelation;
        }

        private IDisposable Subscribe(Action<Frame> subscriber)
        {
            _subscribers.TryAdd(subscriber, null);
            return new Unsubscriber<Frame>(subscriber, (item) => Unsubscribe(item));
        }

        private void Unsubscribe(Delegate subscriber)
        {
            _subscribers.TryRemove(subscriber, out _);
        }

        private Task Publish(object message)
        {
            return Task.Run(() => Parallel.ForEach(_subscribers, (subscriber) =>
            {
                if (subscriber.Key is Action<Frame> frameAction)
                {
                    frameAction((Frame)message);
                    return;
                }
                if (subscriber.Key is Action<Message> messageAction)
                {
                    messageAction((Message)message);
                    return;
                }
            }));
        }

        private Message Convert(DataFrame frame)
        {
            switch (frame.Type)
            {
                case DataFrameType.REQ:
                    return new RequestMessage((ControllerFunction)frame.Payload[0], frame.Payload.Skip(1).ToArray());
                case DataFrameType.RES:
                    return new ResponseMessage((ControllerFunction)frame.Payload[0], frame.Payload.Skip(1).ToArray());
            }
            throw new ProtocolException($"Unexpected frametype: '{frame.Type}'");
        }

        private DataFrame Convert(Message message)
        {
            switch(message)
            {
                case RequestMessage request:
                    return new DataFrame(DataFrameType.REQ, message.Payload);
                case ResponseMessage response:
                    return new DataFrame(DataFrameType.RES, message.Payload);
            }
            throw new ProtocolException($"Unexpected messagetype: '{message.GetType()}'");
        }

        public void Start()
        {
            if (_task != null)
                throw new InvalidOperationException("Broker already started");

            _task = Task.Run(async () =>
            {
                while (!Cancelation.IsCancellationRequested)
                {
                    var frame = default(Frame);
                    try
                    {
                        frame = await _reader.Read(Cancelation);
                        Debug.WriteLine($"Received: {frame}");
                    }
                    catch (ChecksumException ex)
                    {
                        Debug.WriteLine(ex.Message);

                        Debug.WriteLine($"Writing: {Frame.NAK}");
                        await _writer.Write(Frame.NAK, Cancelation);

                        continue;
                    }

                    if (frame is DataFrame dataFrame)
                    {
                        Debug.WriteLine($"Writing: {Frame.ACK}");
                        await _writer.Write(Frame.ACK, Cancelation);

                        var message = Convert(dataFrame);
                        await Publish(message);
                    }

                }
            }, Cancelation);
        }

        public bool Stop(TimeSpan timeout)
        {
            if (_task != null)
            {
                return _task.Wait(timeout);
            }
            return true;
        }

        public async Task Send(RequestMessage message)
        {
            var completion = new TaskCompletionSource<bool>();

            Cancelation.Register(() => completion.TrySetCanceled());

            var subscriber = default(IDisposable);
            subscriber = Subscribe((response) =>
            {
                if (response == Frame.ACK)
                {
                    completion.TrySetResult(true);
                    subscriber.Dispose();
                }
            });

            var dataFrame = Convert(message);
            await _writer.Write(dataFrame, Cancelation);

            await completion.Task;
        }

        public IDisposable Subscribe(Action<Message> subscriber)
        {
            _subscribers.TryAdd(subscriber, null);
            return new Unsubscriber<Message>(subscriber, (item) => Unsubscribe(item));
        }

        private class Unsubscriber<T> : IDisposable
        {
            private readonly Action<T> _subscriber;
            private readonly Action<Action<T>> _onUnsubscribe;

            public Unsubscriber(Action<T> subscriber, Action<Action<T>> onUnsubscribe)
            {
                _subscriber = subscriber;
                _onUnsubscribe = onUnsubscribe;
            }

            public void Dispose()
            {
                _onUnsubscribe(_subscriber);
            }
        }
    }
}
