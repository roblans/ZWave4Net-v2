using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public class ReportSender
    {
        public readonly Node Node;
        public readonly Endpoint Endpoint;

        public ReportSender(Node node, Endpoint endpoint)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }

        public override string ToString()
        {
            return Endpoint.EndpointID != 0 ? $"Node: {Node.NodeID}, Endpoint: {Endpoint.EndpointID}" : $"Node: {Node.NodeID}";
        }
    }
}
