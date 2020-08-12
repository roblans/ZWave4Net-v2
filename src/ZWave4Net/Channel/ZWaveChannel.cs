using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel.Protocol;
using ZWave4Net.Channel.Protocol.Frames;
using ZWave4Net.Diagnostics;
using System.Reactive.Linq;
using ZWave4Net.Utilities;

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
        public int MaxRetryAttempts = 2;

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
            get { return _broker.Messages; }
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

        public async Task Open(bool softReset)
        {
            await Port.Open();

            if (softReset)
            {
                await SoftReset();
            }

            _broker.Run(_cancellationSource.Token);
        }

        internal RequestMessage Encode(ControllerRequest command, byte? callbackID)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

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
                return new RequestMessage(writer.ToPayload());
            }
        }

        internal ControllerMessage Decode(Message message, bool hasCallbackID)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

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

        internal async Task<T> Send<T>(RequestMessage request, IObservable<T> pipeline, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (pipeline == null)
                throw new ArgumentNullException(nameof(pipeline));

            // use timeout only when no cancellationtoken is passed
            var timeout = cancellationToken == default(CancellationToken) ? ResponseTimeout : TimeSpan.MaxValue;

            // number of retransmissions
            var retransmissions = 0;

            while (true)
            {
                // if cancelled then throw
                cancellationToken.ThrowIfCancellationRequested();

                // create a completionsource
                var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                try
                {
                    // set the completion cancelled when the token is cancelled
                    using (cancellationToken.Register(() => completion.TrySetCanceled()))
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
                            await _broker.Send(request, cancellationToken);

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
        internal async Task<T> Send<T>(ControllerRequest request, CancellationToken cancellationToken = default(CancellationToken)) where T : IPayloadSerializable, new()
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

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

            return await Send(Encode(request, null), pipeline, cancellationToken);
        }


        // NodeCommand, no return value. Request followed by:
        // 1) a response from the controller
        // 2) a event from the controller: command deliverd at node)  
        internal async Task Send(byte nodeID, Command command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            // generate new callback
            var callbackID = GetNextCallbackID();

            // create the request
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

            await Send(Encode(controllerRequest, callbackID), pipeline, cancellationToken);
        }

        // NodeCommand, with return value. Request followed by:
        // 1) a response from the controller
        // 2) a event from the controller: command deliverd at node)  
        // 3) a event from the node: return value
        internal async Task<Command> Send(byte nodeID, Command command, byte responseCommandID, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (responseCommandID == 0)
                throw new ArgumentOutOfRangeException(nameof(responseCommandID), responseCommandID, "responseCommandID must be greater than 0");

            // generate new callback
            var callbackID = GetNextCallbackID();

            // create the request
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

            var commandPipeline = Messages
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
                // and select the command
                .Select(response => response.Command);

            // check if the command contains an ecapsulated MultiChannelCommand    
            if (Encapsulation.Flatten(command).OfType<MultiChannelCommand>().Any())
            {
                // extract MultiChannelCommand
                var multiChannelRequestCommand = Encapsulation.Flatten(command).OfType<MultiChannelCommand>().Single();

                // insert MultiChannel filtering  
                commandPipeline = commandPipeline
                // extract the MultiChannelCommand from the response
                .Select(reply => Encapsulation.Flatten(reply).OfType<MultiChannelCommand>().Single())
                // verify if the MultiChannelCommand is the correct one
                .Where(reply => reply.CommandClass == multiChannelRequestCommand.CommandClass && reply.CommandID == multiChannelRequestCommand.CommandID)
                // verify if the endpoint the correct one
                .Where(reply => reply.SourceEndpointID == multiChannelRequestCommand.TargetEndpointID);
            }

            // get the most inner command
            var requestCommand = Encapsulation.Flatten(command).Last();

            commandPipeline = commandPipeline
            // select the inner most command
            .Select(reply => Encapsulation.Flatten(reply).Last())
            // verify if the response conmmand is the correct one
            .Where(reply => reply.CommandClass == requestCommand.CommandClass && reply.CommandID == responseCommandID);

            return await Send(Encode(controllerRequest, callbackID), commandPipeline, cancellationToken);
        }

        internal IObservable<Command> ReceiveNodeEvents(byte nodeID, Command command)
        {
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var commandPipeline = Messages
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
            // and select the command
            .Select(response => response.Command);

            // check if the command contains an ecapsulated MultiChannelCommand    
            if (Encapsulation.Flatten(command).OfType<MultiChannelCommand>().Any())
            {
                // extract MultiChannelCommand
                var multiChannelRequestCommand = Encapsulation.Flatten(command).OfType<MultiChannelCommand>().Single();

                commandPipeline = commandPipeline
                // extract the MultiChannelCommand from the response
                .Select(reply => Encapsulation.Flatten(reply).OfType<MultiChannelCommand>().SingleOrDefault())
                // verify if present
                .Where(reply => reply != null)
                // verify if the MultiChannelCommand is the correct one
                .Where(reply => reply.CommandClass == multiChannelRequestCommand.CommandClass && reply.CommandID == multiChannelRequestCommand.CommandID)
                // verify if the endpoint is the correct one
                .Where(reply => reply.SourceEndpointID == multiChannelRequestCommand.SourceEndpointID);
            }

            // get the most inner command
            var requestCommand = Encapsulation.Flatten(command).Last();

            return commandPipeline
            // select the inner most command
            .Select(reply => Encapsulation.Flatten(reply).Last())
            // verify if the response conmmand is the correct one
            .Where(reply => reply.CommandClass == requestCommand.CommandClass && reply.CommandID == requestCommand.CommandID);
        }

        internal IObservable<NodeUpdate> ReceiveNodeUpdates(byte nodeID)
        {
            if (nodeID == 0)
                throw new ArgumentOutOfRangeException(nameof(nodeID), nodeID, "nodeID must be greater than 0");

            return Messages
            // decode the response
            .Select(message => Decode(message, hasCallbackID: false))
            // we only want events (no responses)
            .OfType<ControllerEvent>()
            // verify function
            .Where(@event => Equals(@event.Function, Function.ApplicationUpdate))
            // deserialize the received payload to a NodeUpdate
            .Select(@event => @event.Payload.Deserialize<NodeUpdate>())
            // verify node
            .Where(update => update.NodeID == nodeID);
        }

        public async Task Close()
        {
            _cancellationSource.Cancel();

            await _broker;
            await Port.Close();
        }
    }
}
