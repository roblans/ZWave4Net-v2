using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.Diagnostics
{
    public static partial class Extentions
    {
        public static void LogDebug(this ILogger logger, object state)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.Log(LogLevel.Debug, state);
        }

        public static void LogInfo(this ILogger logger, object state)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.Log(LogLevel.Info, state);
        }

        public static void LogWarning(this ILogger logger, object state)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.Log(LogLevel.Warning, state);
        }

        public static void LogError(this ILogger logger, object state)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.Log(LogLevel.Error, state);
        }

        public static void LogCritical(this ILogger logger, object state)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.Log(LogLevel.Critical, state);
        }
    }
}
