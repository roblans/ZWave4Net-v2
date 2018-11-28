using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.Channel.Protocol.Frames;
using ZWave4Net.Diagnostics;

namespace ZWave4Net.Channel
{
    public class MessageChannel
    {
        private readonly ILogger _logger = Logging.Factory.CreatLogger("Channel");
        private readonly MessageBroker _broker;
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        public readonly ISerialPort Port;
        
        public MessageChannel(ISerialPort port)
        {
            Port = port ?? throw new ArgumentNullException(nameof(port));

            _broker = new MessageBroker(port);
        }

        public async Task Open()
        {
            await Port.Open();

            _broker.Run(_cancellationSource.Token);
        }

        private async Task<ControllerMessage> Send(HostMessage request, Func<ControllerMessage, bool> predicate, CancellationToken cancellation = default(CancellationToken))
        {
            // number of retransmissions
            var retransmissions = 0;

            // return only on response or exception
            while (true)
            {
                // create completion source, will be completed on an expected response
                var completion = new TaskCompletionSource<ControllerMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

                // callback, called on every message received
                void onVerifyResponse(ControllerMessage response)
                {
                    // matching response?
                    if (predicate(response))
                    {
                        // yes, so set complete
                        completion.TrySetResult(response);
                    }
                };

                using (var subscription = _broker.Subscribe(onVerifyResponse))
                {
                    if (retransmissions == 0)
                        _logger.LogDebug($"Sending: {request}");
                    else
                        _logger.LogWarning($"Resending: {request}, attempt: {retransmissions}");

                    await _broker.Send(request, cancellation);

                    var timeout = Task.Delay(request.Timeout, cancellation);
                    _logger.LogDebug($"Wait for response or timeout");

                    if ((await Task.WhenAny(completion.Task, timeout)) == completion.Task)
                    {
                        var response = await completion.Task;
                        _logger.LogDebug($"Received: {response}");

                        return response;
                    }
                    else
                    {
                        _logger.LogWarning($"Timeout while waiting for a response");

                        // Timeout
                        if (retransmissions >= ProtocolSettings.MaxRetryAttempts)
                            throw new TimeoutException("Timeout while waiting for a response");
                    }
                }

                retransmissions++;
            }
        }

        public async Task<ResponseMessage> Send(HostMessage request, CancellationToken cancellation = default(CancellationToken))
        {
            return (ResponseMessage)await Send(request, (response) => response.Function == request.Function, cancellation);
        }

        public async Task<EventMessage> Send(HostMessage request, Func<EventMessage, bool> predicate, CancellationToken cancellation = default(CancellationToken))
        {
            return (EventMessage)await Send(request, (response) =>
            {
                if (response.Function == request.Function && response is EventMessage @event)
                {
                    return predicate(@event);
                }
                return false;

            }, cancellation);
        }

        public async Task Close()
        {
            _cancellationSource.Cancel();

            await _broker;
            await Port.Close();
        }
    }
}
