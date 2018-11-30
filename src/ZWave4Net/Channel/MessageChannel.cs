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
        private static readonly object _lock = new object();
        private static byte _callbackID = 0;

        private readonly ILogger _logger = Logging.Factory.CreatLogger("Channel");
        private readonly MessageBroker _broker;
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        public readonly ISerialPort Port;
        
        public MessageChannel(ISerialPort port)
        {
            Port = port ?? throw new ArgumentNullException(nameof(port));

            _broker = new MessageBroker(port);
        }

        public static byte GetNextCallbackID()
        {
            lock (_lock) { return _callbackID = (byte)((_callbackID % 255) + 1); }
        }

        private async Task SoftReset()
        {
            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.1 Initializatio
            // 1) Close host serial port if it is open
            // 2) Open the host serial port at 115200 baud 8N1.
            // 3) Send the NAK
            // 4) Send SerialAPI command: FUNC_ID_SERIAL_API_SOFT_RESET
            // 5) Wait 1.5s

            var message = new HostMessage(Function.SerialApiSoftReset);
            var frame = MessageBroker.Encode(message);

            var writer = new FrameWriter(Port);
            await writer.Write(Frame.NAK, CancellationToken.None);
            await writer.Write(frame, CancellationToken.None);

            await Task.Delay(1500);
        }

        public async Task Open()
        {
            await Port.Open();

            await SoftReset();

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
                void onValidateResponse(ControllerMessage response)
                {
                    // matching response?
                    if (predicate(response))
                    {
                        // yes, so set complete
                        completion.TrySetResult(response);
                    }
                };

                using (var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
                {
                    // use timeout from request
                    timeoutCancellation.CancelAfter(request.Timeout);
                    timeoutCancellation.Token.Register(() => completion.TrySetCanceled());

                    // start listening for received messages, call onVerifyResponse for every controllermessage
                    using (var subscription = _broker.Subscribe(onValidateResponse))
                    {
                        if (retransmissions == 0)
                            _logger.LogDebug($"Sending: {request}");
                        else
                            _logger.LogWarning($"Resending: {request}, attempt: {retransmissions}");

                        // send the request
                        await _broker.Send(request, cancellation);

                        _logger.LogDebug($"Wait for response or timeout");
                        try
                        {
                            // wait for validated response
                            var response = await completion.Task;

                            _logger.LogDebug($"Received: {response}");

                            // done, so return response
                            return response;
                        }
                        catch (TaskCanceledException) when (cancellation.IsCancellationRequested)
                        {
                            // operation was externally canceled, so rethrow
                            throw;
                        }
                        catch (TaskCanceledException) when (timeoutCancellation.IsCancellationRequested)
                        {
                            // operation timed-out
                            _logger.LogWarning($"Timeout while waiting for a response");

                            // check if maximum retries reached
                            if (retransmissions >= request.MaxRetryAttempts)
                                throw new TimeoutException("Timeout while waiting for a response");
                        }
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
