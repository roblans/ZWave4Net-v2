using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public abstract class Report
    {
        public ReportSender Sender { get; private set; }

        internal static T Create<T>(Node node, Endpoint endpoint, Payload payload) where T : Report, new()
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var report = new T()
            {
                Sender = new ReportSender(node, endpoint),
            };

            using (var reader = new PayloadReader(payload))
            {
                report.Read(reader);
            }
            return report;
        }

        protected abstract void Read(PayloadReader reader);

        public override string ToString()
        {
            return $"{GetType().Name}: Sender: {Sender}";
        }
    }
}
