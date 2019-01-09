using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    /// <summary>
    /// The Version command class may be used to obtain the Z-Wave library type, the Z-Wave protocol version used by the application, the individual command class versions used by the application and the vendor specific application version from a Z-Wave enabled device
    /// </summary>
    public interface IVersion
    {
        /// <summary>
        /// The Version Get Command is used to request the library type, protocol version and application version from a device that supports the Version Command Class
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<VersionReport> Get(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// The Version Command Class Get Command is used to request the individual command class versions from a device
        /// </summary>
        /// <param name="commandClass">This field specifies which command class identifier is being requested</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<VersionCommandClassReport> GetCommandClass(CommandClass commandClass, CancellationToken cancellationToken = default(CancellationToken));
    }
}