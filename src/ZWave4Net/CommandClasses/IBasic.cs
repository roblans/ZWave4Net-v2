using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave.CommandClasses
{
    /// <summary>
    /// The IBasic command class allows a controlling device to operate the primary functionality of a supporting device without any further knowledge.
    /// </summary>
    public interface IBasic
    {
        /// <summary>
        /// Request the status of a supporting device
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<BasicReport> Get(CancellationToken cancellationToken = default);

        /// <summary>
        /// Set a value in a supporting device.
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task Set(byte value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Advertises the status of the primary functionality of the device
        /// </summary>
        IObservable<BasicReport> Reports { get; }
    }
}
