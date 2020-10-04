using System.Threading;
using System.Threading.Tasks;

namespace ZWave.CommandClasses
{
    /// <summary>
    /// The ManufacturerSpecific command class advertises manufacturer specific information
    /// </summary>
    public interface IManufacturerSpecific
    {
        /// <summary>
        /// Use the Manufacturer Specific Get Command to request manufacturer specific information from a node
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<ManufacturerSpecificReport> Get(CancellationToken cancellationToken = default(CancellationToken));
    }
}