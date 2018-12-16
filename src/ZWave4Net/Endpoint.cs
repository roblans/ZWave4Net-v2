using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel;
using ZWave4Net.CommandClasses;

namespace ZWave4Net
{
    public class Endpoint : IEquatable<Endpoint>
    {
        public readonly Address Address;
        public readonly ZWaveController Controller;

        public Endpoint(ZWaveController controller, Address address)
        {
            Controller = controller;
            Address = address;
        }

        public byte NodeID
        {
            get { return Address.NodeID; }
        }

        public byte EndpointID
        {
            get { return Address.EndpointID; }
        }

        public override string ToString()
        {
            return $"{Address}";
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
                   Address == other.Address;
        }

        public override int GetHashCode()
        {
            var hashCode = 1833194505;
            hashCode = hashCode * -1521134295 + Address.GetHashCode();
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
