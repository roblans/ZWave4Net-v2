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
        public static Task<T> Send<T>(this ZWaveChannel channel, Command command) where T : IPayload, new()
        {
            return channel.Send<T>(command, null, default(CancellationToken));
        }

        public static Task<T> Send<T>(this ZWaveChannel channel, Command command, CancellationToken cancellation) where T : IPayload, new()
        {
            return channel.Send<T>(command, null, cancellation);
        }

        public static Task<T> Send<T>(this ZWaveChannel channel, Command command, Func<T, bool> predicate) where T : IPayload, new()
        {
            return channel.Send<T>(command, predicate, default(CancellationToken));
        }

        public static Task<Payload> Send(this ZWaveChannel channel, Command command)
        {
            return channel.Send<Payload>(command, null, default(CancellationToken));
        }

        public static Task<Payload> Send(this ZWaveChannel channel, Command command, CancellationToken cancellation)
        {
            return channel.Send<Payload>(command, null, cancellation);
        }

        public static Task<Payload> Send(this ZWaveChannel channel, Command command, Func<Payload, bool> predicate)
        {
            return channel.Send<Payload>(command, predicate, default(CancellationToken));
        }

        public static Task<Payload> Send(this ZWaveChannel channel, Command command, Func<Payload, bool> predicate, CancellationToken cancellation)
        {
            return channel.Send<Payload>(command, predicate, cancellation);
        }
    }
}
