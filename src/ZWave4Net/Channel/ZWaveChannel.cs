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
    public class ZWaveChannel
    {
        private static readonly object _lock = new object();
        private static byte _callbackID = 0;

        private readonly ILogger _logger = Logging.Factory.CreatLogger("Channel");
        private readonly MessageBroker _broker;
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        public readonly ISerialPort Port;
        
        public ZWaveChannel(ISerialPort port)
        {
            Port = port ?? throw new ArgumentNullException(nameof(port));

            _broker = new MessageBroker(port);
        }

        private static byte GetNextCallbackID()
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

            var message = new HostMessage(new Payload((byte)Function.SerialApiSoftReset));
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
                    timeoutCancellation.CancelAfter(request.ResponseTimeout);
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

        public async Task<T> Send<T>(Command command, Func<T, bool> predicate, CancellationToken cancellation) where T : IPayload, new()
        {
            // create writer to serialize te request
            using (var writer = new PayloadWriter())
            {
                // write the function
                writer.WriteByte((byte)command.Function);

                // does the command has payload?
                if (command.Payload != null)
                {
                    // yes, so write the payload
                    writer.WriteObject(command.Payload);
                }

                //  if callback is required then generate a new callback 
                var callbackID = command.UseCallbackID ? GetNextCallbackID() : default(byte?);
                if (callbackID != null)
                {
                    // write the callback
                    writer.WriteByte(callbackID.Value);
                }

                // create a hostmessage, use the serialized payload  
                var hostMessage = new HostMessage(writer.GetPayload());
                
                // custom timeout specified?
                if (command.ResponseTimeout != null)
                {
                    // yes, so override default
                    hostMessage.ResponseTimeout = command.ResponseTimeout.Value;
                }

                // custom retry specified?
                if (command.MaxRetryAttempts != null)
                {
                    // yes, so override default
                    hostMessage.MaxRetryAttempts = command.MaxRetryAttempts.Value;
                }
                
                // send the result, the callback delegate will be called on every message received from the controller
                var responseMessage = await Send(hostMessage, (controllerMessage) =>
                {
                    // check if this response matches the request
                    if (!TryParseMatchingResponse<T>(controllerMessage, command.Function, callbackID, out var payload))
                        return false;

                    // if we have a custom predicate the use it to verify the response
                    if (predicate != null && !predicate(payload))
                        return false;

                    // OK, matching response, so return true
                    return true;
                },
                cancellation);


                return ParseMatchingResponse<T>(responseMessage, command.Function, callbackID);
            }
        }

        private bool TryParseMatchingResponse<T>(ControllerMessage response, Function function, byte? callbackID, out T payload) where T : IPayload, new()
        {
            payload = default(T);

            using (var reader = new PayloadReader(response.Payload))
            {
                // check if expected function
                if (!object.Equals((Function)reader.ReadByte(), function))
                    return false;

                // does the command has a callbackID?
                if (callbackID != null)
                {
                    // check if expected callback
                    if (!object.Equals(reader.ReadByte(), callbackID))
                        return false;
                }

                // OK, seems we have a matching response. Deserialize the payload
                payload = reader.ReadObject<T>();

                // we're done
                return true;
            }
        }

        private T ParseMatchingResponse<T>(ControllerMessage message, Function function, byte? callbackID) where T : IPayload, new()
        {
            using (var reader = new PayloadReader(message.Payload))
            {

                // check if expected function
                if (!object.Equals((Function)reader.ReadByte(), function))
                    throw new ReponseFormatException("Function mismatch");

                // does the request has a callbackID?
                if (callbackID != null)
                {
                    // check if expected callback
                    if (!object.Equals(reader.ReadByte(), callbackID.Value))
                        throw new ReponseFormatException("CallbackID mismatch");
                }

                // OK, seems we have a matching response. Deserialize the payload
                return reader.ReadObject<T>();
            }
        }

        public async Task Close()
        {
            _cancellationSource.Cancel();

            await _broker;
            await Port.Close();
        }
    }
}
