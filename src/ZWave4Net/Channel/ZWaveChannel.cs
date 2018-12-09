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

            var message = new RequestMessage(new ByteArray((byte)Function.SerialApiSoftReset));
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

        private RequestMessage Encode(ControllerRequest command, byte? callbackID)
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
                return new RequestMessage(writer.ToByteArray());
            }
        }

        private ControllerMessage Decode(Message message, bool hasCallbackID)
        {
            // create reader to deserialize the request
            using (var reader = new PayloadReader(message.Payload))
            {
                // read the function
                var function = (Function)reader.ReadByte();

                var callbackID = hasCallbackID ? reader.ReadByte() : default(byte?);

                // read the payload
                var payload = new ByteArray(reader.ReadBytes(reader.Length - reader.Position));

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

        private async Task<T> Send<T>(RequestMessage request, IObservable<T> pipeline,  CancellationToken cancellation = default(CancellationToken)) where T : IPayload, new()
        {
            var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (pipeline.Subscribe
            (
                (T element) => completion.TrySetResult(element),
                (Exception ex) => completion.TrySetException(ex)
            ))
            {
                // send the request
                await _broker.Send(request, cancellation);

                // wait for response
                return await completion.Task;
            }
        }

        // ControllerRequest: request followed by one response from the controller
        public async Task<T> Send<T>(ControllerRequest request, CancellationToken cancellation = default(CancellationToken)) where T : IPayload, new()
        {
            var callbackID = request.UseCallbackID ? GetNextCallbackID() : default(byte?);

            var pipeline = _broker.GetObservable()
                .Select(element => Decode(element, callbackID.HasValue))
                .OfType<ControllerResponse>()
                .Where(element => Equals(element.Function, request.Function))
                .Where(element => Equals(element.CallbackID, callbackID))
                .Timeout(request.ResponseTimeout)
                .Select(element => element.Payload);

            var response = await Send(Encode(request, callbackID), pipeline, cancellation);

            return response.Deserialize<T>();
        }

        // ControllerRequest: request followed by one or more events. The passed predicate is called on every event (progress)
        // When the caller returns true on the predicate then the command is considered complete
        // The result of the completed task is the payload of the last event received
        public async Task<T> Send<T>(ControllerRequest request, Func<T, bool> predicate, CancellationToken cancellation = default(CancellationToken)) where T : IPayload, new()
        {
            var callbackID = request.UseCallbackID ? GetNextCallbackID() : default(byte?);

            var pipeline = _broker.GetObservable()
                .Select(element => Decode(element, callbackID.HasValue))
                .OfType<ControllerEvent>()
                .Where(element => Equals(element.Function, request.Function))
                .Where(element => Equals(element.CallbackID, callbackID))
                .Timeout(request.ResponseTimeout)
                .Select(element => element.Payload)
                .Select(element => element.Deserialize<T>())
                .Where(predicate);

            return await Send(Encode(request, callbackID), pipeline, cancellation);
        }

        // NodeCommand, no return value. Request followed by:
        // 1) a response from the controller
        // 2) a event from the controller: command deliverd at node)  
        public async Task Send(byte nodeID, NodeCommand command, CancellationToken cancellation = default(CancellationToken))
        {
            var callbackID = GetNextCallbackID();

            var nodeRequest = new NodeRequest(nodeID, command);

            var controllerRequest = new ControllerRequest(Function.SendData, nodeRequest.Serialize())
            {
                UseCallbackID = true,
            };

            var responsePipeline = _broker.GetObservable()
                .Select(element => Decode(element, false))
                .OfType<ControllerResponse>()
                .Where(element => Equals(element.Function, controllerRequest.Function));

            var pipeline = _broker.GetObservable()
                .SkipUntil(responsePipeline)
                .Select(element => Decode(element, true))
                .OfType<ControllerEvent>()
                .Where(element => Equals(element.Function, controllerRequest.Function))
                .Where(element => Equals(element.CallbackID, callbackID))
                .Select(element => element.Payload.Deserialize<NodeResponse>())
                .Where(element => element.NodeID == 0);

            await Send(Encode(controllerRequest, callbackID), pipeline, cancellation);
        }

        // NodeCommand, with return value. Request followed by:
        // 1) a response from the controller
        // 2) a event from the controller: command deliverd at node)  
        // 3) a event from the node: return value
        public async Task<T> Send<T>(byte nodeID, NodeCommand command, byte responseCommandID, CancellationToken cancellation = default(CancellationToken)) where T : IPayload, new()
        {
            var completion = new TaskCompletionSource<ByteArray>(TaskCreationOptions.RunContinuationsAsynchronously);
            var callbackID = GetNextCallbackID();

            var nodeRequest = new NodeRequest(nodeID, command);

            var controllerRequest = new ControllerRequest(Function.SendData, nodeRequest.Serialize())
            {
                UseCallbackID = true,
            };

            var responsePipeline = _broker.GetObservable()
                .Select(element => Decode(element, false))
                .OfType<ControllerResponse>()
                .Where(element => Equals(element.Function, controllerRequest.Function));

            var eventPipeline = _broker.GetObservable()
                .SkipUntil(responsePipeline)
                .Select(element => Decode(element, true))
                .OfType<ControllerEvent>()
                .Where(element => Equals(element.Function, controllerRequest.Function))
                .Where(element => Equals(element.CallbackID, callbackID));

            var pipeline = _broker.GetObservable()
                .SkipUntil(responsePipeline)
                .SkipUntil(eventPipeline)
                .Select(element => Decode(element, false))
                .OfType<ControllerEvent>()
                .Where(element => Equals(element.Function, Function.ApplicationCommandHandler))
                .Select(element => element.Payload.Deserialize<NodeResponse>())
                .Where(element => element.NodeID == nodeID)
                .Select(element => element.Payload.Deserialize<NodeReply>())
                .Where(element => element.CommandID == responseCommandID)
                .Select(element => element.Payload);

            var response = await Send(Encode(controllerRequest, callbackID), pipeline, cancellation);

            return response.Deserialize<T>();
        }

        public async Task Close()
        {
            _cancellationSource.Cancel();

            await _broker;
            await Port.Close();
        }
    }
}
