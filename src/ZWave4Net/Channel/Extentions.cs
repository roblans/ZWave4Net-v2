using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public static partial class Extentions
    {
        public static async Task<ResponseMessage> Send(this MessageChannel channel, HostMessage request, CancellationToken cancellation = default(CancellationToken))
        {
            return (ResponseMessage)await channel.Send(request, (response) => response.Function == request.Function, cancellation);
        }

        public static async Task<EventMessage> Send(this MessageChannel channel, HostMessage request, Func<EventMessage, bool> predicate, CancellationToken cancellation = default(CancellationToken))
        {
            return (EventMessage)await channel.Send(request, (response) =>
            {
                if (response.Function == request.Function && response is EventMessage @event)
                {
                    return predicate(@event);
                }
                return false;

            }, cancellation);
        }


        public static async Task<TResponse> Send<TResponse>(this MessageChannel channel, HostMessage request, CancellationToken cancellation = default(CancellationToken)) where TResponse : IPayloadReadable, new()
        {
            var response = await channel.Send(request, cancellation);

            using (var reader = new PayloadReader(response.Payload))
            {
                return reader.ReadObject<TResponse>();
            }
        }
    }
}
