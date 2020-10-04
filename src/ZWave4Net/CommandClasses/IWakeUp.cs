using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave.CommandClasses
{
    /// <summary>
    /// The Wake Up command class allows a battery-powered device to notify another device (always listening), that it is awake and ready to receive any queued commands.
    /// </summary>
    public interface IWakeUp
    {
        /// <summary>
        /// This command is used to request the Wake Up Interval and destination of a node.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<WakeUpIntervalReport> GetInterval(CancellationToken cancellationToken = default);

        /// <summary>
        /// This command is used to configure the Wake Up interval and destination of a node
        /// </summary>
        /// <param name="interval">The Wake Up periods for the receiving node</param>
        /// <param name="targetNodeID">The Wake Up destination NodeID</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task SetInterval(TimeSpan interval, byte targetNodeID, CancellationToken cancellationToken = default);

        /// <summary>
        /// This command is used to notify a supporting node that it MAY return to sleep to minimize power consumption.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task NoMoreInformation(CancellationToken cancellationToken);

        /// <summary>
        /// Notifies its Wake Up destination that it is awake
        /// </summary>
        IObservable<WakeUpNotificationReport> Reports { get; }
    }
}
