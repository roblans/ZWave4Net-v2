using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.Channel.Protocol.Frames;
using ZWave4Net.Diagnostics;
using System.Reactive.Linq;

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

        internal static byte GetNextCallbackID()
        {
            lock (_lock) { return _callbackID = (byte)((_callbackID % 255) + 1); }
        }

        internal IObservable<Message> Messages
        {
            get { return _broker.GetObservable(); }
        }

        private async Task SoftReset()
        {
            // INS12350-Serial-API-Host-Appl.-Prg.-Guide | 6.1 Initializatio
            // 1) Close host serial port if it is open
            // 2) Open the host serial port at 115200 baud 8N1.
            // 3) Send the NAK
            // 4) Send SerialAPI command: FUNC_ID_SERIAL_API_SOFT_RESET
            // 5) Wait 1.5s

            var message = new RequestMessage(new PayloadBytes((byte)Function.SerialApiSoftReset));
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

        internal RequestMessage Encode(ControllerRequest command, byte? callbackID)
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
                return new RequestMessage(writer.ToPayloadBytes());
            }
        }

        internal ControllerMessage Decode(Message message, bool hasCallbackID)
        {
            // create reader to deserialize the request
            using (var reader = new PayloadReader(message.Payload))
            {
                // read the function
                var function = (Function)reader.ReadByte();

                var callbackID = hasCallbackID ? reader.ReadByte() : default(byte?);

                // read the payload
                var payload = new PayloadBytes(reader.ReadBytes(reader.Length - reader.Position));

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

        internal async Task<T> Send<T>(RequestMessage request, IObservable<T> pipeline, TimeSpan timeout, int maxRetryAttempts, CancellationToken cancellation = default(CancellationToken)) where T : IPayloadSerializable, new()
        {
#if DEBUG
            timeout = TimeSpan.FromSeconds(60);
#endif
            // number of retransmissions
            var retransmissions = 0;

            while (true)
            {
                // if cancelled then throw
                cancellation.ThrowIfCancellationRequested();

                try
                {
                    // create a completionsource
                    var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

                    // set the completion cancelled when the token is cancelled
                    using (cancellation.Register(() => completion.TrySetCanceled()))
                    {
                        // subscribe to response pipeline, with timout
                        using (pipeline.Timeout(timeout).Subscribe
                        (
                            // OK, pipeline completed, set result
                            (element) => completion.TrySetResult(element),
                            // Exception, pipeline failed, set error
                            (ex) => completion.TrySetException(ex)
                        ))
                        {
                            // send the request
                            await _broker.Send(request, cancellation);

                            // wait for response
                            return await completion.Task;
                        }
                    }
                }
                catch (TimeoutException)
                {
                    // operation timed-out
                    _logger.LogWarning($"Timeout while waiting for a response");

                    // throw exception when max retransmissions reached
                    if (retransmissions >= ProtocolSettings.MaxRetryAttempts)
                        throw new TimeoutException("Timeout while waiting for a response");
                }
                catch (TransmissionException ex)
                {
                    // tranmission failure
                    _logger.LogWarning(ex.Message);

                    // throw exception when max retransmissions reached
                    if (retransmissions >= ProtocolSettings.MaxRetryAttempts)
                        throw;
                }

                retransmissions++;
            }
        }

        // ControllerRequest: request followed by one response from the controller
        public async Task<T> Send<T>(ControllerRequest request, CancellationToken cancellation = default(CancellationToken)) where T : IPayloadSerializable, new()
        {
            // create the response pipeline
            var pipeline = Messages
                // decode the response
                .Select(message => Decode(message, false))
                // we only want responses (no events)
                .OfType<ControllerResponse>()
                // verify matching function
                .Where(message => Equals(message.Function, request.Function))
                // and finally deserialize the received payload
                .Select(message => message.Payload.Deserialize<T>());

            return await Send(Encode(request, null), pipeline, request.ResponseTimeout, request.MaxRetryAttempts, cancellation);
        }


        // ControllerRequest: request followed by one events.
        // The result of the completed task is the payload of the response function received
        public async Task<T> Send<T>(ControllerRequest request, Function responseFunction, CancellationToken cancellation = default(CancellationToken)) where T : IPayloadSerializable, new()
        {
            // generate new callback
            var callbackID = GetNextCallbackID();

            var responsePipeline = Messages
                // decode the response
                .Select(message => Decode(message, false))
                // we only want responses (no events)
                .OfType<ControllerResponse>()
                // verify matching function
                .Where(message => Equals(message.Function, request.Function))
                // unknown what the 0x01 byte means
                .Where(message => message.Payload[0] == 0x01);

            var pipeline = Messages
                // wait until the response pipeline has finished
                .SkipUntil(responsePipeline)
                // decode the response
                .Select(message => Decode(message, false))
                // we only want events (no responses)
                .OfType<ControllerEvent>()
                // verify matching function
                .Where(message => Equals(message.Function, responseFunction))
                // deserialize the received payload to a NodeCommandCompleted
                .Select(message => message.Payload.Deserialize<T>());

            return await Send(Encode(request, null), pipeline, request.ResponseTimeout, request.MaxRetryAttempts, cancellation);
        }

        // NodeCommand, no return value. Request followed by:
        // 1) a response from the controller
        // 2) a event from the controller: command deliverd at node)  
        public async Task Send(byte nodeID, NodeCommand command, CancellationToken cancellation = default(CancellationToken))
        {
            var nodeRequest = new NodeRequest(nodeID, command);

            var controllerRequest = new ControllerRequest(Function.SendData, nodeRequest.Serialize());

            // generate new callback
            var callbackID = GetNextCallbackID();

            var responsePipeline = Messages
                // decode the response
                .Select(message => Decode(message, false))
                // we only want responses (no events)
                .OfType<ControllerResponse>()
                // verify matching function
                .Where(message => Equals(message.Function, controllerRequest.Function))
                // unknown what the 0x01 byte means
                .Where(message => message.Payload[0] == 0x01);

            var pipeline = Messages
                // wait until the response pipeline has finished
                .SkipUntil(responsePipeline)
                // decode the response
                .Select(message => Decode(message, true))
                // we only want events (no responses)
                .OfType<ControllerEvent>()
                // verify matching function
                .Where(message => Equals(message.Function, controllerRequest.Function))
                // verify mathing callback
                .Where(message => Equals(message.CallbackID, callbackID))
                // deserialize the received payload to a NodeCommandCompleted
                .Select(message => message.Payload.Deserialize<NodeCommandCompleted>())
                // verify the state
                .Verify(message => message.TransmissionState == TransmissionState.CompleteOK, message => new TransmissionException(message.TransmissionState));

            await Send(Encode(controllerRequest, callbackID), pipeline, command.ReplyTimeout, command.MaxRetryAttempts, cancellation);
        }

        // NodeCommand, with return value. Request followed by:
        // 1) a response from the controller
        // 2) a event from the controller: command deliverd at node)  
        // 3) a event from the node: return value
        public async Task<T> Send<T>(byte nodeID, NodeCommand command, byte responseCommandID, CancellationToken cancellation = default(CancellationToken)) where T : IPayloadSerializable, new()
        {
            var nodeRequest = new NodeRequest(nodeID, command);

            var controllerRequest = new ControllerRequest(Function.SendData, nodeRequest.Serialize());

            // generate new callback
            var callbackID = GetNextCallbackID();

            var responsePipeline = Messages
                // decode the response
                .Select(message => Decode(message, false))
                // we only want responses (no events)
                .OfType<ControllerResponse>()
                // verify matching function
                .Where(message => Equals(message.Function, controllerRequest.Function))
                // unknown what the 0x01 byte means
                .Where(message => message.Payload[0] == 0x01);


            var eventPipeline = Messages
                // decode the response
                .Select(message => Decode(message, true))
                // we only want events (no responses)
                .OfType<ControllerEvent>()
                // verify matching function
                .Where(message => Equals(message.Function, controllerRequest.Function))
                // verify matching callback
                .Where(message => Equals(message.CallbackID, callbackID))
                // deserialize the received payload to a NodeCommandCompleted
                .Select(message => message.Payload.Deserialize<NodeCommandCompleted>())
                // verify the state
                .Verify(message => message.TransmissionState == TransmissionState.CompleteOK, message => new TransmissionException(message.TransmissionState));

            var pipeline = Messages
                // wait until the response pipeline has finished
                .SkipUntil(responsePipeline)
                // wait until the event pipeline has finished
                .SkipUntil(eventPipeline)
                // decode the response
                .Select(message => Decode(message, false))
                // we only want events (no responses)
                .OfType<ControllerEvent>()
                // after SendData controler will respond with ApplicationCommandHandler
                .Where(message => Equals(message.Function, Function.ApplicationCommandHandler))
                // deserialize the received payload to a NodeResponse
                .Select(message => message.Payload.Deserialize<NodeResponse>())
                // verify if the responding node is the correct one
                .Where(message => message.NodeID == nodeID)
                // deserialize the received payload to a NodeReply
                .Select(message => message.Payload.Deserialize<NodeReply>())
                // verify if the response conmmand is the correct on
                .Where(message => message.CommandID == responseCommandID)
                // finally deserialize the payload
                .Select(message => message.Payload.Deserialize<T>());

            return await Send(Encode(controllerRequest, callbackID), pipeline, command.ReplyTimeout, command.MaxRetryAttempts, cancellation);
        }

        public IObservable<T> Receive<T>(byte nodeID, byte commandID) where T : IPayloadSerializable, new()
        {
            return Messages
            // decode the response
            .Select(message => Decode(message, false))
            // we only want events (no responses)
            .OfType<ControllerEvent>()
            // after SendData controler will respond with ApplicationCommandHandler
            .Where(message => Equals(message.Function, Function.ApplicationCommandHandler))
            // deserialize the received payload to a NodeResponse
            .Select(message => message.Payload.Deserialize<NodeResponse>())
            // verify if the responding node is the correct one
            .Where(message => message.NodeID == nodeID)
            // deserialize the received payload to a NodeReply
            .Select(message => message.Payload.Deserialize<NodeReply>())
            // verify if the response conmmand is the correct on
            .Where(message => message.CommandID == commandID)
            // finally deserialize the payload
            .Select(message => message.Payload.Deserialize<T>());
        }

        public async Task Close()
        {
            _cancellationSource.Cancel();

            await _broker;
            await Port.Close();
        }
    }
}
