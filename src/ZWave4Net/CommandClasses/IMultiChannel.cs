using System.Threading;
using System.Threading.Tasks;

namespace ZWave.CommandClasses
{
    /// <summary>
    /// The Multi Channel command class is used to address one or more end points in a Multi Channel device. 
    /// </summary>
    public interface IMultiChannel
    {
        /// <summary>
        /// The Multi Channel End Point Get Command is used to query the number of End Points implemented by the node.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<MultiChannelEndpointsReport> GetEndpoints(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// The Multi Channel Capability Get Command is used to query the capabilities of End Points
        /// </summary>
        /// <param name="endpointID">The ID of the endpoint</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<MultiChannelCapabilityReport> GetCapability(byte endpointID, CancellationToken cancellationToken = default(CancellationToken));
    }
}