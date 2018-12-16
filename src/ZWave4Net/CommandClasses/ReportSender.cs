using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public class ReportSender
    {
        public readonly byte NodeID;
        public readonly byte EndpointID;
        public readonly string Name;

        public ReportSender(byte nodeID, byte endpointID)
        {
            NodeID = nodeID;
            EndpointID = endpointID;
            Name = ZWaveController.GetEndpointName(NodeID, EndpointID);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
