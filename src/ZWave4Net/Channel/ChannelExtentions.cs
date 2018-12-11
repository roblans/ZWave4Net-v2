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
            var callbackID = ZWaveChannel.GetNextCallbackID();

            var command = new ControllerRequest(Function.RequestNodeNeighborUpdate, new PayloadBytes(nodeID))
            {
                ResponseTimeout = TimeSpan.FromSeconds(5),
            };

            var request = channel.Encode(command, callbackID);


            var pipeline = channel.Messages
                // decode the response
                .Select(message => channel.Decode(message, true))
                // we only want events (no responses)
                .OfType<ControllerEvent>()
                // verify matching function
                .Where(message => Equals(message.Function, command.Function))
                // verify mathing callback (can be null)
                .Where(message => Equals(message.CallbackID, callbackID))
                // deserialize the received payload
                .Select(message => message.Payload.Deserialize<PayloadBytes>())
                // report progress
                .Do(payload => progress?.Report((NeighborUpdateStatus)payload[0]))
                // and check for final state
                .Where(payload => (NeighborUpdateStatus)payload[0] == NeighborUpdateStatus.Done || (NeighborUpdateStatus)payload[0] == NeighborUpdateStatus.Failed);

                // send request
                var response = await channel.Send(request, pipeline, command.ResponseTimeout, command.MaxRetryAttempts, cancellationToken);

                // return the status of the final response
                return (NeighborUpdateStatus)response[0];
        }
    }
}
