﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave.CommandClasses
{
    /// <summary>
    /// The Binary Switch command class is used to control devices with On/Off or Enable/Disable capability
    /// </summary>
    public interface ISwitchBinary
    {
        /// <summary>
        /// The Get command is used to request the status of a device with On/Off or Enable/Disable capability
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<SwitchBinaryReport> Get(CancellationToken cancellationToken = default);

        /// <summary>
        /// The Set command is used to set a value
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task Set(bool value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Advertises the status of a device with On/Off or Enable/Disable capability.
        /// </summary>
        IObservable<SwitchBinaryReport> Reports { get; }
    }
}
