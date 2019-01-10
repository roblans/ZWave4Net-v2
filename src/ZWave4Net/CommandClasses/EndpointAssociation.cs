using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public class EndpointAssociation
    {
        public byte NodeID { get; }
        public byte EndpointID { get; }

        public EndpointAssociation(byte nodeID, byte endpointID)
        {
            NodeID = nodeID;
            EndpointID = endpointID;
        }

        public override string ToString()
        {
            return $"Node: {NodeID}, Endpoint: {EndpointID}";
        }
    }
}
