using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave4Net.CommandClasses
{
    /// <summary>
    /// The Powerlevel Command Class defines RF transmit power controlling Commands useful when installing or testing a network
    /// </summary>
    public interface IPowerlevel
    {
        /// <summary>
        /// The Powerlevel Get Command is used to request the current power level value. 
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<PowerlevelReport> Get(CancellationToken cancellation = default(CancellationToken));

        /// <summary>
        /// The Powerlevel Set Command is used to set the power level indicator value, which should be used by the node when transmitting RF, and the timeout for this power level indicator value before returning the power level defined by the application
        /// </summary>
        /// <param name="Level">This field indicates the power level value</param>
        /// <param name="timeout">The time in seconds (1..255) the node should keep the Power level before resetting to normalPower level</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task Set(Powerlevel Level, TimeSpan timeout, CancellationToken cancellation = default(CancellationToken));
    }
}