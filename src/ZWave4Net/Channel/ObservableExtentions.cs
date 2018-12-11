using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public static partial class ObservableExtentions
    {
        public static IObservable<T> Verify<T>(this IObservable<T> source, Predicate<T> predicate, Func<T, Exception> getError)
        {
            return Observable.Create<T>(o =>
                source.Subscribe(x => 
                {
                    if (predicate(x))
                    {
                        o.OnNext(x);
                    }
                    else
                    {
                        o.OnError(getError(x));
                    }
                },
                o.OnError,
                o.OnCompleted));
        }
    }
}