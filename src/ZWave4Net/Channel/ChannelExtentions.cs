using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel.Protocol;
using System.Reactive.Linq;

namespace ZWave4Net.Channel
{
    public static partial class ChannelExtentions
    {
        public static async Task<NeighborUpdateStatus> SendRequestNeighborUpdate(this ZWaveChannel channel, byte nodeID, IProgress<NeighborUpdateStatus> progress, CancellationToken cancellationToken = default(CancellationToken))
        {
            var responseTimeout = TimeSpan.FromSeconds(5);
            var callbackID = ZWaveChannel.GetNextCallbackID();

            var command = new ControllerRequest(Function.RequestNodeNeighborUpdate, new Payload(nodeID));
            var request = channel.Encode(command, callbackID);

            var pipeline = channel.Messages
                // decode the response
                .Select(message => channel.Decode(message, hasCallbackID: true))
                // we only want events (no responses)
                .OfType<ControllerEvent>()
                // verify matching function
                .Where(@event => Equals(@event.Function, command.Function))
                // verify mathing callback (can be null)
                .Where(@event => Equals(@event.CallbackID, callbackID))
                // deserialize the received payload
                .Select(@event => @event.Payload.Deserialize<Payload>())
                // report progress
                .Do(payload => progress?.Report((NeighborUpdateStatus)payload[0]))
                // and check for final state
                .Where(payload => (NeighborUpdateStatus)payload[0] == NeighborUpdateStatus.Done || (NeighborUpdateStatus)payload[0] == NeighborUpdateStatus.Failed);

            // send request
            var response = await channel.Send(request, pipeline, responseTimeout, channel.MaxRetryAttempts, cancellationToken);

            // return the status of the final response
            return (NeighborUpdateStatus)response[0];
        }

        public static async Task<NodeInfoData> SendRequestNodeInfo(this ZWaveChannel channel, byte nodeID, CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new ControllerRequest(Function.RequestNodeInfo, new Payload(nodeID));
            var request = channel.Encode(command, null);

            var responsePipeline = channel.Messages
                // decode the response
                .Select(message => channel.Decode(message, hasCallbackID: false))
                // we only want responses (no events)
                .OfType<ControllerResponse>()
                // verify matching function
                .Where(response => Equals(response.Function, command.Function))
                // unknown what the 0x01 byte means, probably: ready, finished, OK
                .Where(response => response.Payload[0] == 0x01);

            var pipeline = channel.Messages
                // wait until the response pipeline has finished
                .SkipUntil(responsePipeline)
                // decode the response
                .Select(message => channel.Decode(message, hasCallbackID: false))
                // we only want events (no responses)
                .OfType<ControllerEvent>()
                // verify matching function
                .Where(@event => Equals(@event.Function, Function.ApplicationUpdate))
                // deserialize the received payload to a NodeUpdate
                .Select(@event => @event.Payload.Deserialize<NodeUpdate<NodeInfoData>>())
                // verify the state
                .Verify(@event => @event.State == NodeUpdateState.InfoReceived, @event => new OperationFailedException($"RequestNodeInfo failed with state: {@event.State}"))
                // deserialize to nodeinfo
                .Select(update => update.Data);

            // send request
            return await channel.Send(request, pipeline, cancellationToken);
        }

    }
}
