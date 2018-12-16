using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public class Address : IEquatable<Address>
    {
        public readonly byte NodeID;
        public readonly byte EndpointID;

        public Address(byte nodeID, byte endpointID = 0)
        {
            NodeID = nodeID;
            EndpointID = endpointID;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Address);
        }

        public bool Equals(Address other)
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

        public override string ToString()
        {
            return EndpointID != 0 ? $"{NodeID}.{EndpointID}" : $"{NodeID}";
        }

        public static bool operator ==(Address address1, Address address2)
        {
            return EqualityComparer<Address>.Default.Equals(address1, address2);
        }

        public static bool operator !=(Address address1, Address address2)
        {
            return !(address1 == address2);
        }
    }
}
