using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.Diagnostics
{
    public static class Logging
    {
        private static ILogFactory _factory;

        public static ILogFactory Factory
        {
            get { return _factory ?? (_factory = new LogFactory()); }
        }
    }
}
