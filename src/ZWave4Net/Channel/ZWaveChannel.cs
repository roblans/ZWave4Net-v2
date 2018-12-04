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

            var message = new RequestMessage(new Payload((byte)Function.SerialApiSoftReset));
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

        private RequestMessage Encode(ControllerCommand command, byte? callbackID)
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
                return new RequestMessage(writer.GetPayload());
            }
        }

        private ControllerNotification Decode(ControllerMessage message, bool hasCallbackID)
        {
            // create reader to deserialize the request
            using (var reader = new PayloadReader(message.Payload))
            {
                // read the function
                var function = (Function)reader.ReadByte();

                var callbackID = default(byte?);
                if (hasCallbackID && function != Function.ApplicationCommandHandler)
                {
                    // read the (optional callback
                    callbackID = reader.ReadByte();
                }

                // read the payload
                var payload = new Payload(reader.ReadBytes(reader.Length - reader.Position));

                if (message is ResponseMessage)
                {
                    return new ControllerResponse(function, callbackID, payload);
                }
                if (message is EventMessage)
                {
                    return new ControllerEvent(function, callbackID, payload);
                }

                throw new InvalidOperationException("Unknown message type");
            }
        }

        private async Task<Payload> Send(ControllerCommand command, IEnumerable<Func<ControllerNotification, bool>> predicates, CancellationToken cancellation = default(CancellationToken))
        {
            // number of retransmissions
            var retransmissions = 0;

            // return only on response or exception
            while (true)
            {
                var callbackID = command.UseCallbackID ? GetNextCallbackID() : default(byte?);
                var request = Encode(command, callbackID);

                    // send the request
                    await _broker.Send(request, cancellation);

                    using (var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
                    {
                        // use timeout from request
                        timeoutCancellation.CancelAfter(command.ResponseTimeout);

                    try
                    {
                        var responses = new List<Payload>();

                        using (var receiver = new MessageReceiver(_broker))
                        {
                            foreach (var predicate in predicates)
                            {
                                var completion = new TaskCompletionSource<Payload>(TaskCreationOptions.RunContinuationsAsynchronously);

                                using (timeoutCancellation.Token.Register(() => completion.TrySetCanceled()))
                                {

                                    await receiver.Until((message) =>
                                    {
                                        var response = Decode(message, callbackID != null);

                                        _logger.LogError("test");

                                        if (response.Function != Function.ApplicationCommandHandler)
                                        {
                                            if (!Equals(callbackID, response.CallbackID))
                                                return false;
                                        }

                                        if (predicate(response))
                                        {
                                            completion.TrySetResult(response.Payload);
                                            return true;
                                        }


                                        return false;

                                    }, timeoutCancellation.Token);

                                    responses.Add(await completion.Task);
                                }
                            }

                            return responses.Last();
                        }
                    }
                    catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                    {
                        // operation was externally canceled, so rethrow
                        throw;
                    }
                    catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested)
                    {
                        // operation timed-out
                        _logger.LogWarning($"Timeout while waiting for a response on: {command}");

                        if (retransmissions >= command.MaxRetryAttempts)
                            throw new TimeoutException($"Timeout while waiting for a response on: {command}");
                    }
                }
                retransmissions++;
            }
        }

        // ControllerCommand: Request followed by one response from the controller
        public async Task<T> Send<T>(ControllerCommand command, CancellationToken cancellation = default(CancellationToken)) where T : IPayload, new()
        {
            var predicates = new Func<ControllerNotification, bool>[]
            {
                // check is notification is a Response and the Function matches the request
                (ControllerNotification response) => response is ControllerResponse && Equals(command.Function, response.Function),
            };

            var payload = await Send(command, predicates, cancellation);
            using (var reader = new PayloadReader(payload))
            {
                return reader.ReadObject<T>();
            }
        }

        // ControllerCommand: Request followed by one or more events. The passed predicate is called on every event (progress)
        // When the caller returns true on the predicate then the command is considered complete
        // The result of the completed task is the payload of the last event received
        public async Task<T> Send<T>(ControllerCommand command, Func<T, bool> predicate, CancellationToken cancellation = default(CancellationToken)) where T : IPayload, new()
        {
            var predicates = new Func<ControllerNotification, bool>[]
            {
                // check is notification is an Event, the Function matches the request and the predicate returns true
                (ControllerNotification response) =>
                {
                    if (response is ControllerEvent && Equals(command.Function, response.Function))
                    {
                        using (var reader = new PayloadReader(response.Payload))
                        {
                            return predicate(reader.ReadObject<T>());
                        }
                    }
                    return false;
                }
            };

            var payload = await Send(command, predicates, cancellation);
            using (var reader = new PayloadReader(payload))
            {
                return reader.ReadObject<T>();
            }
        }


        // NodeCommand, no return value. Request followed by:
        // 1) a response from the controller
        // 2) a event from the node (command deliverd at node)  
        public async Task Send(byte nodeID, NodeCommand nodeCommand, CancellationToken cancellation = default(CancellationToken))
        {
            using (var writer = new PayloadWriter())
            {
                writer.WriteByte(nodeID);
                writer.WriteObject(nodeCommand);
                writer.WriteByte((byte)(TransmitOptions.Ack | TransmitOptions.AutoRoute | TransmitOptions.Explore));

                var command = new ControllerCommand(Function.SendData, writer.GetPayload())
                {
                    UseCallbackID = true,
                };

                var predicates = new Func<ControllerNotification, bool>[]
                {
                    // phase 1: Response from controller 
                    (ControllerNotification response) => response is ControllerResponse && Equals(command.Function, response.Function),

                    // phase 2: Event received, command delivered at Node
                    (ControllerNotification response) =>
                    {
                        if (response is ControllerEvent && Equals(command.Function, response.Function))
                        {
                            var transmissionState = (TransmissionState)response.Payload[0];
                            if (transmissionState == TransmissionState.CompleteOK)
                                return true;

                            _logger.LogError($"Transmission failure: {transmissionState}");
                        }
                        return false;
                     }
                };

                await Send(command, predicates, cancellation);
            }
        }

        // NodeCommand, with return value. Request followed by:
        // 1) a response from the controller
        // 2) a event from the node (command deliverd at node)  
        public async Task<Payload> Send(byte nodeID, NodeCommand nodeCommand, byte responseCommandID, CancellationToken cancellation = default(CancellationToken))
        {
            using (var writer = new PayloadWriter())
            {
                writer.WriteByte(nodeID);
                writer.WriteObject(nodeCommand);
                writer.WriteByte((byte)(TransmitOptions.Ack | TransmitOptions.AutoRoute | TransmitOptions.Explore));

                var command = new ControllerCommand(Function.SendData, writer.GetPayload())
                {
                    UseCallbackID = true,
                };

                var predicates = new Func<ControllerNotification, bool>[]
                {
                    // phase 1: Response from controller 
                    (ControllerNotification response) => response is ControllerResponse && Equals(command.Function, response.Function),

                    // phase 2: Event received, command delivered at Node
                    (ControllerNotification response) =>
                    {
                        if (response is ControllerEvent && Equals(command.Function, response.Function))
                        {
                            var transmissionState = (TransmissionState)response.Payload[0];
                            if (transmissionState == TransmissionState.CompleteOK)
                                return true;

                            _logger.LogError($"Transmission failure: {transmissionState}");
                        }
                        return false;
                    },
                    // phase 3: Event received, return value from node
                    (ControllerNotification response) =>
                    {
                        if (response is ControllerEvent && Equals(Function.ApplicationCommandHandler, response.Function))
                        {
                            using(var reader = new PayloadReader(response.Payload))
                            {
                                var status = reader.ReadByte();

                                var receiveStatus = ReceiveStatus.None;

                                if ((status & 0x01) > 0)
                                    receiveStatus |= ReceiveStatus.RoutedBusy;
                                if ((status & 0x02) > 0)
                                    receiveStatus |= ReceiveStatus.LowPower;
                                if ((status & 0x0C) == 0x00)
                                    receiveStatus |= ReceiveStatus.TypeSingle;
                                if ((status & 0x0C) == 0x01)
                                    receiveStatus |= ReceiveStatus.TypeBroad;
                                if ((status & 0x0C) == 0x10)
                                    receiveStatus |= ReceiveStatus.TypeMulti;
                                if ((status & 0x10) > 0)
                                    receiveStatus |= ReceiveStatus.TypeExplore;
                                if ((status & 0x40) > 0)
                                    receiveStatus |= ReceiveStatus.ForeignFrame;

                                var responseNodeID = reader.ReadByte();
                                if (responseNodeID != nodeID)
                                    return false;

                                var nodeResponse = reader.ReadObject<NodeResponse>();
                                if (nodeResponse.ClassID != (byte)nodeCommand.Class)
                                    return false;
                                if (nodeResponse.CommandID != responseCommandID)
                                    return false;

                                return true;
                            }
                        }
                        return false;
                    },
                };

                var payload = await Send(command, predicates, cancellation);
                using (var reader = new PayloadReader(payload))
                {
                    // skip receivestatus and responseNodeID
                    reader.SkipBytes(2);

                    // read the response
                    var nodeResponse = reader.ReadObject<NodeResponse>();

                    // but only return the payload
                    return nodeResponse.Payload;
                }
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
