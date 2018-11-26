using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.Channel.Protocol
{
    public class ControllerCommandDispatcher
    {
        private readonly MessageBroker _broker;
        private readonly CancellationTokenSource _cancellationSource;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        public ControllerCommandDispatcher(IDuplexStream stream)
        {
            _broker = new MessageBroker(stream);
        }

        public Task Initialize()
        {
            _broker.Run(_cancellationSource.Token);
            return Task.FromResult(true);
        }

        public async Task Shutdown()
        {
            _cancellationSource.Cancel();
            await _broker;
        }

        public async Task<byte[]> Send(ControllerCommand command, Action<byte[]> progress = null, CancellationToken cancellation = default(CancellationToken))
        {
            await _sendLock.WaitAsync(cancellation);
            try
            {
                return null;
                //// return only on ACK or Exception
                //while (true)
                //{
                //    // create completion source, will be completed on an expected response
                //    var completion = new TaskCompletionSource<Message>();

                //    // callback, called on every message received
                //    void onVerifyResponse(Message message)
                //    {
                //        // one of the expected responses?
                //        if (message is ResponseMessage response && )
                //        {
                //            // yes, so set complete
                //            completion.TrySetResult(frame);
                //        }
                //    };

                //    // start listening for received frames, call onVerifyResponse for every received Message 
                //    using (var subscription = _broker.Subcribe<Message>(onVerifyResponse))
                //    {
                //    }
                //}
            }
            finally
            {
                _sendLock.Release();
            }
        }
    }
}
