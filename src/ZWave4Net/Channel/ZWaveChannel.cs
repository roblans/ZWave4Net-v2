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

        public TimeSpan ResponseTimeout = TimeSpan.FromSeconds(5);
        public int MaxRetryAttempts = 0;

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

        public RequestMessage Encode(ControllerRequest command, byte? callbackID)
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

        public ControllerMessage Decode(Message message, bool hasCallbackID)
        {
            // create reader to deserialize the request
            using (var reader = new PayloadReader(message.Payload))
            {
                // read the function
                var function = (Function)reader.ReadByte();

                var callbackID = hasCallbackID ? reader.ReadByte() : default(byte?);

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

        public Task<T> Send<T>(RequestMessage request, IObservable<T> pipeline, CancellationToken cancellation = default(CancellationToken)) where T : IPayloadSerializable, new()
        {
            return Send(request, pipeline, ResponseTimeout, MaxRetryAttempts, cancellation);
        }

        public async Task<T> Send<T>(RequestMessage request, IObservable<T> pipeline, TimeSpan timeout, int maxRetryAttempts, CancellationToken cancellation = default(CancellationToken)) where T : IPayloadSerializable, new()
        {
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
                    _logger.LogWarning($"Timeout while waiting for a response on: {request}");

                    // throw exception when max retransmissions reached
                    if (retransmissions >= ProtocolSettings.MaxRetryAttempts)
                        throw new TimeoutException($"Timeout while waiting for a response on: {request}");
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
                .Select(message => Decode(message, hasCallbackID: false))
                // we only want responses (no events)
                .OfType<ControllerResponse>()
                // verify matching function
                .Where(response => Equals(response.Function, request.Function))
                // and finally deserialize the received payload
                .Select(response => response.Payload.Deserialize<T>());

            return await Send(Encode(request, null), pipeline, cancellation);
        }


        // NodeCommand, no return value. Request followed by:
        // 1) a response from the controller
        // 2) a event from the controller: command deliverd at node)  
        public async Task Send(byte nodeID, NodeCommand command, CancellationToken cancellation = default(CancellationToken))
        {
            // generate new callback
            var callbackID = GetNextCallbackID();

            var nodeRequest = new NodeRequest(nodeID, command);
            var controllerRequest = new ControllerRequest(Function.SendData, nodeRequest.Serialize());

            var responsePipeline = Messages
                // decode the response
                .Select(message => Decode(message, hasCallbackID: false))
                // we only want responses (no events)
                .OfType<ControllerResponse>()
                // verify matching function
                .Where(response => Equals(response.Function, controllerRequest.Function))
                // unknown what the 0x01 byte means, probably: ready, finished, OK
                .Where(response => response.Payload[0] == 0x01);

            var pipeline = Messages
                // wait until the response pipeline has finished
                .SkipUntil(responsePipeline)
                // decode the response
                .Select(message => Decode(message, hasCallbackID: true))
                // we only want events (no responses)
                .OfType<ControllerEvent>()
                // verify matching function
                .Where(@event => Equals(@event.Function, controllerRequest.Function))
                // verify mathing callback
                .Where(@event => Equals(@event.CallbackID, callbackID))
                // deserialize the received payload to a NodeCommandCompleted
                .Select(@event => @event.Payload.Deserialize<NodeCommandCompleted>())
                // verify the state
                .Verify(completed => completed.TransmissionState == TransmissionState.CompleteOK, completed => new TransmissionException(completed.TransmissionState));

            await Send(Encode(controllerRequest, callbackID), pipeline, cancellation);
        }

        // NodeCommand, with return value. Request followed by:
        // 1) a response from the controller
        // 2) a event from the controller: command deliverd at node)  
        // 3) a event from the node: return value
        public async Task<T> Send<T>(byte nodeID, NodeCommand command, byte responseCommandID, CancellationToken cancellation = default(CancellationToken)) where T : IPayloadSerializable, new()
        {
            // generate new callback
            var callbackID = GetNextCallbackID();

            var nodeRequest = new NodeRequest(nodeID, command);
            var controllerRequest = new ControllerRequest(Function.SendData, nodeRequest.Serialize());

            var responsePipeline = Messages
                // decode the response
                .Select(message => Decode(message, hasCallbackID: false))
                // we only want responses (no events)
                .OfType<ControllerResponse>()
                // verify matching function
                .Where(response => Equals(response.Function, controllerRequest.Function))
                // unknown what the 0x01 byte means, probably: ready, finished, OK
                .Where(response => response.Payload[0] == 0x01);


            var eventPipeline = Messages
                // decode the response
                .Select(message => Decode(message, hasCallbackID: true))
                // we only want events (no responses)
                .OfType<ControllerEvent>()
                // verify matching function
                .Where(@event => Equals(@event.Function, controllerRequest.Function))
                // verify matching callback
                .Where(@event => Equals(@event.CallbackID, callbackID))
                // deserialize the received payload to a NodeCommandCompleted
                .Select(@event => @event.Payload.Deserialize<NodeCommandCompleted>())
                // verify the state
                .Verify(completed => completed.TransmissionState == TransmissionState.CompleteOK, completed => new TransmissionException(completed.TransmissionState));

            var pipeline = Messages
                // wait until the response pipeline has finished
                .SkipUntil(responsePipeline)
                // wait until the event pipeline has finished
                .SkipUntil(eventPipeline)
                // decode the response
                .Select(message => Decode(message, hasCallbackID: false))
                // we only want events (no responses)
                .OfType<ControllerEvent>()
                // after SendData controler will respond with ApplicationCommandHandler
                .Where(@event => Equals(@event.Function, Function.ApplicationCommandHandler))
                // deserialize the received payload to a NodeResponse
                .Select(@event => @event.Payload.Deserialize<NodeResponse>())
                // verify if the responding node is the correct one
                .Where(response => response.NodeID == nodeID)
                // deserialize the received payload to a NodeReply
                .Select(response => response.Payload.Deserialize<NodeReply>())
                // verify if the response conmmand is the correct on
                .Where(reply => reply.CommandID == responseCommandID)
                // finally deserialize the payload
                .Select(reply => reply.Payload.Deserialize<T>());

            return await Send(Encode(controllerRequest, callbackID), pipeline, cancellation);
        }

        public IObservable<T> NodeEvents<T>(byte nodeID, byte commandID) where T : IPayloadSerializable, new()
        {
            return Messages
            // decode the response
            .Select(message => Decode(message, hasCallbackID: false))
            // we only want events (no responses)
            .OfType<ControllerEvent>()
            // after SendData controler will respond with ApplicationCommandHandler
            .Where(@event => Equals(@event.Function, Function.ApplicationCommandHandler))
            // deserialize the received payload to a NodeResponse
            .Select(@event => @event.Payload.Deserialize<NodeResponse>())
            // verify if the responding node is the correct one
            .Where(response => response.NodeID == nodeID)
            // deserialize the received payload to a NodeReply
            .Select(response => response.Payload.Deserialize<NodeReply>())
            // verify if the response conmmand is the correct on
            .Where(reply => reply.CommandID == commandID)
            // finally deserialize the payload
            .Select(reply => reply.Payload.Deserialize<T>());
        }

        public IObservable<NodeUpdateInfo> NodeUpdates(byte nodeID)
        {
            return Messages
            // decode the response
            .Select(message => Decode(message, hasCallbackID: false))
            // we only want events (no responses)
            .OfType<ControllerEvent>()
            // verify function
            .Where(@event => Equals(@event.Function, Function.ApplicationUpdate))
            // deserialize the received payload to a NodeUpdate
            .Select(@event => @event.Payload.Deserialize<NodeUpdate<NodeUpdateInfo>>())
            // verify node
            .Where(update => update.NodeID == nodeID)
            // verify state
            .Where(update => update.State == NodeUpdateState.InfoReceived)
            // select the data
            .Select(update => update.Data);
        }

        public async Task Close()
        {
            _cancellationSource.Cancel();

            await _broker;
            await Port.Close();
        }
    }
}
