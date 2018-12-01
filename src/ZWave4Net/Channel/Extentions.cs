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
        public static Task<T> Send<T>(this MessageChannel channel, IRequestCommand request) where T : IPayloadReadable, new()
        {
            return channel.Send<T>(request, null, default(CancellationToken));
        }

        public static Task<T> Send<T>(this MessageChannel channel, IRequestCommand request, CancellationToken cancellation) where T : IPayloadReadable, new()
        {
            return channel.Send<T>(request, null, cancellation);
        }

        public static Task<T> Send<T>(this MessageChannel channel, IRequestCommand request, Func<T, bool> predicate) where T : IPayloadReadable, new()
        {
            return channel.Send<T>(request, predicate, default(CancellationToken));
        }

        public static async Task<T> Send<T>(this MessageChannel channel, IRequestCommand request, Func<T, bool> predicate, CancellationToken cancellation) where T : IPayloadReadable, new()
        {
            using (var writer = new PayloadWriter())
            {
                request.WriteTo(writer);

                var hostMessage = new HostMessage(writer.GetPayload());

                var result = await channel.Send(hostMessage, (controllerMessage) =>
                {
                    using (var reader = new PayloadReader(controllerMessage.Payload))
                    {
                        var response = new ResponseCommand<T>
                        {
                            UseCallbackID = request.UseCallbackID
                        };
                        response.ReadFrom(reader);

                        if (!object.Equals(response.Function, request.Function))
                            return false;

                        if (!object.Equals(response.CallbackID, request.CallbackID))
                            return false;

                        if (predicate != null && !predicate(response.Payload))
                            return false;

                        return true;
                    }
                },
                cancellation);

                using (var reader = new PayloadReader(result.Payload))
                {
                    var response = new ResponseCommand<T>
                    {
                        UseCallbackID = request.UseCallbackID
                    };
                    response.ReadFrom(reader);
                    return response.Payload;
                }
            }
        }

        public static Task<Payload> Send(this MessageChannel channel, IRequestCommand request)
        {
            return channel.Send<Payload>(request, null, default(CancellationToken));
        }

        public static Task<Payload> Send(this MessageChannel channel, IRequestCommand request, CancellationToken cancellation)
        {
            return channel.Send<Payload>(request, null, cancellation);
        }

        public static Task<Payload> Send(this MessageChannel channel, IRequestCommand request, Func<Payload, bool> predicate)
        {
            return channel.Send<Payload>(request, predicate, default(CancellationToken));
        }

        public static Task<Payload> Send(this MessageChannel channel, IRequestCommand request, Func<Payload, bool> predicate, CancellationToken cancellation)
        {
            return channel.Send<Payload>(request, predicate, cancellation);
        }
    }
}
