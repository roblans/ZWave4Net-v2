using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZWave.CommandClasses
{
    /// <summary>
    /// The Multilevel Sensor Command Class is used to control a multilevel sensor
    /// </summary>
    public interface ISensorMultiLevel
    {
        /// <summary>
        /// The Multilevel Sensor Get Command is used to request the level of a multilevel sensor.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<SensorMultiLevelReport> Get(CancellationToken cancellationToken = default);

        /// <summary>
        /// The Multilevel Sensor Get Command is used to request the level of a multilevel sensor.
        /// </summary>
        /// <param name="type">Specifies the type of sensor </param>
        /// <param name="type">The Scale used to indicate what unit the sensor uses</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<SensorMultiLevelReport> Get(SensorType type, byte scale = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// Advertises a sensor reading
        /// </summary>
        IObservable<SensorMultiLevelReport> Reports { get; }
    }
}
