using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel;
using ZWave4Net.CommandClasses;

namespace ZWave4Net
{
    public class Endpoint : IEquatable<Endpoint>
    {
        public readonly byte NodeID;
        public readonly byte EndpointID;
        public readonly ZWaveController Controller;

        public Endpoint(ZWaveController controller, byte nodeID, byte endpointID)
        {
            Controller = controller;
            NodeID = nodeID;
            EndpointID = endpointID;
        }

        public override string ToString()
        {
            return $"{NodeID:D3}.{EndpointID:D3}";
        }

        protected ZWaveChannel Channel
        {
            get { return Controller.Channel; }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Endpoint);
        }

        public bool Equals(Endpoint other)
        {
            return other != null &&
                   NodeID == other.NodeID &&
                   EndpointID == other.EndpointID;
        }

        public override int GetHashCode()
        {
            var hashCode = 1833194505;
            hashCode = hashCode * -1521134295 + NodeID.GetHashCode();
            hashCode = hashCode * -1521134295 + EndpointID.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Endpoint endpoint1, Endpoint endpoint2)
        {
            return EqualityComparer<Endpoint>.Default.Equals(endpoint1, endpoint2);
        }

        public static bool operator !=(Endpoint endpoint1, Endpoint endpoint2)
        {
            return !(endpoint1 == endpoint2);
        }
    }
}
