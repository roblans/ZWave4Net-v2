using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol.Frames
{
    public static class ProtocolSettings
    {
        public static TimeSpan ACKWaitTime = TimeSpan.FromMilliseconds(1500);
        public static TimeSpan DataFrameWaitTime = TimeSpan.FromMilliseconds(1500);
        public static TimeSpan RetryDelayWaitTime = TimeSpan.FromMilliseconds(100);
        public static TimeSpan RetryAttemptWaitTime = TimeSpan.FromMilliseconds(1000);
        public static int MaxRetryAttempts = 3;
    }
}
