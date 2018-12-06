using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class MessageReceiver : IDisposable
    {
        private IDisposable _subscription;
        private BlockingCollection<Message> _queue = new BlockingCollection<Message>();

        public MessageReceiver(MessageBroker broker)
        {
            //_subscription = broker.Subscribe(OnMessage);
        }

        private void OnMessage(Message message)
        {
            _queue.Add(message);
        }

        public Task Until(Predicate<Message> predicate, CancellationToken cancellation)
        {
            return Task.Run(() =>
            {
                try
                {
                    foreach (var message in _queue.GetConsumingEnumerable(cancellation))
                    {
                        if (predicate(message))
                        {
                            _queue.CompleteAdding();
                            _subscription.Dispose();
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            });
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _subscription.Dispose();
        }
    }
}
