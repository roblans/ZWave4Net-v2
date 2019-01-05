using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    public interface IMultiChannel
    {
        Task<MultiChannelEndpointsReport> GetEndpoints(CancellationToken cancellation = default(CancellationToken));
    }
}