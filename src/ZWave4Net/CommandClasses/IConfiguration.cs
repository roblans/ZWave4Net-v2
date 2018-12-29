using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    /// <summary>
    /// The Configuration interface allows product specific configuration parameters to be changed
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// The Configuration Get Command is used to query the value of a configuration parameter. 
        /// </summary>
        /// <param name="parameter">The parameter number</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<ConfigurationReport> Get(byte parameter, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// The Configuration Set Command is used to set the value of a configuration parameter.
        /// </summary>
        /// <param name="parameter">The parameter number</param>
        /// <param name="value">The value of the parameter, must be an integral value</param>
        /// <param name="size">The size of the parameter, must be 1, 2 or 4</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task Set(byte parameter, object value, byte size, CancellationToken cancellationToken = default(CancellationToken));
    }
}