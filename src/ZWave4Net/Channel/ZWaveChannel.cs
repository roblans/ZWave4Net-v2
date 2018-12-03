using System;
using System.Collections.Generic;
using System.Linq;
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

        private HostMessage Encode(ControllerNotification command, byte? callbackID)
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

                if (callbackID != null)
                {
                    // write the callback
                    writer.WriteByte(callbackID.Value);
                }

                // create a hostmessage, use the serialized payload  
                return new HostMessage(writer.GetPayload());
            }
        }

        private ControllerNotification<T> Decode<T>(ControllerMessage message, bool hasCallbackID) where T : IPayload, new()
        {
            // create reader to deserialize the request
            using (var reader = new PayloadReader(message.Payload))
            {
                // read the function
                var function = (Function)reader.ReadByte();

                // read the (optional callback
                var callbackID = hasCallbackID ? reader.ReadByte() : default(byte?);

                // read the payload
                var payload = reader.ReadObject<T>();

                if (message is ResponseMessage)
                {
                    return new ControllerResponse<T>(function, callbackID, payload);
                }
                if (message is EventMessage)
                {
                    return new ControllerEvent<T>(function, callbackID, payload);
                }

                throw new InvalidOperationException("Unknown message type");
            }
        }

        private async Task<T> Send<T>(ControllerNotification command, IEnumerable<Func<ControllerNotification<T>, bool>> predicates, CancellationToken cancellation = default(CancellationToken)) where T : IPayload, new()
        {
            // number of retransmissions
            var retransmissions = 0;

            // return only on response or exception
            while (true)
            {
                var callbackID = command.UseCallbackID ? GetNextCallbackID() : default(byte?);
                var request = Encode(command, callbackID);

                using (var receiver = new MessageReceiver(_broker))
                {
                    // send the request
                    await _broker.Send(request, cancellation);

                    using (var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
                    {
                        // use timeout from request
                        timeoutCancellation.CancelAfter(command.ResponseTimeout);

                        try
                        {
                            var responses = new List<T>();

                            foreach (var predicate in predicates)
                            {
                                var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                                timeoutCancellation.Token.Register(() => completion.TrySetCanceled());

                                await receiver.Until((message) =>
                                {
                                    var response = Decode<T>(message, callbackID != null);

                                    if (response.Function == Function.RequestNodeNeighborUpdate)
                                    {
                                        _logger.LogError(response);
                                    }

                                    if (!Equals(callbackID, response.CallbackID))
                                        return false;

                                    if (predicate(response))
                                    {
                                        completion.TrySetResult(response.Payload);
                                        return true;
                                    }

                                    return false;

                                }, timeoutCancellation.Token);

                                responses.Add(await completion.Task);
                            }

                            return responses.Last();
                        }
                        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                        {
                            // operation was externally canceled, so rethrow
                            throw;
                        }
                        catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested)
                        {
                            // operation timed-out
                            _logger.LogWarning($"Timeout while waiting for a response");

                            if (retransmissions >= command.MaxRetryAttempts)
                                throw new TimeoutException("Timeout while waiting for a response");
                        }
                    }
                }
                retransmissions++;
            }
        }

        // Request followed by one response
        public async Task<T> Send<T>(ControllerNotification command, CancellationToken cancellation = default(CancellationToken)) where T : IPayload, new()
        {
            var predicates = new Func<ControllerNotification<T>, bool>[]
            {
                // check is notification is a Response and the Function matches the request
                (ControllerNotification<T> response) => response is ControllerResponse<T> && Equals(command.Function, response.Function),
            };

            return await Send<T>(command, predicates, cancellation);
        }

        // Request followed by one or more events. The passed predicate is called on every event (progress)
        // When the caller returns true on the predicate then the command is considered complete
        // The result of the completed task is the payload of the last event received
        public async Task<T> Send<T>(ControllerNotification command, Func<T, bool> predicate, CancellationToken cancellation = default(CancellationToken)) where T : IPayload, new()
        {
            var predicates = new Func<ControllerNotification<T>, bool>[]
            {
                // check is notification is an Event, the Function matches the request and the predicate returns true
                (ControllerNotification<T> response) => response is ControllerEvent<T> && Equals(command.Function, response.Function) && predicate(response.Payload),
            };

            return await Send<T>(command, predicates, cancellation);
        }

        //public async Task<T> Send<T>(byte nodeID, NodeCommand command, CancellationToken cancellation = default(CancellationToken)) where T : IPayload, new()
        //{
        //    using (var writer = new PayloadWriter())
        //    {
        //        writer.WriteByte(nodeID);
        //        writer.WriteObject(command);
        //        writer.WriteByte((byte)(TransmitOptions.Ack | TransmitOptions.AutoRoute | TransmitOptions.Explore));

        //        var controllerCommand = new ControllerCommand(Function.SendData, writer.GetPayload())
        //        {
        //            UseCallbackID = true,
        //        };

        //        var response = await Send<Payload>(controllerCommand, null, cancellation);

        //        using (var reader = new PayloadReader(response))
        //        {
        //            var state = (TransmissionState)reader.ReadByte();

        //            if (state == TransmissionState.CompleteOK)
        //                _logger.LogDebug($"TransmissionState: {state}");
        //            else
        //                _logger.LogError($"TransmissionState: {state}");
        //        }
        //    }
        //    return default(T);
        //}

        public async Task Close()
        {
            _cancellationSource.Cancel();

            await _broker;
            await Port.Close();
        }
    }
}
