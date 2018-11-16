using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public static class SerialProtocol
    {
        public static TimeSpan ACKWaitTime = TimeSpan.FromMilliseconds(1500);
        public static TimeSpan SOFWaitTime = TimeSpan.FromMilliseconds(1500);
        public static TimeSpan RetryWaitTimeOffset = TimeSpan.FromMilliseconds(100);
        public static TimeSpan RetryWaitTimeAttempt = TimeSpan.FromMilliseconds(1000);
        public static int MaxRetryAttempts = 3;
    }
}
