using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel;
using ZWave4Net.CommandClasses;

namespace ZWave4Net
{
    public interface IEndpoint
    {

    }

    public class Endpoint : IEquatable<Endpoint>, IEndpoint
    {
        public readonly byte EndpointID;
        public readonly Node Node;
        public readonly ZWaveController Controller;

        protected Endpoint(ZWaveController controller)
        {
            Node = (Node)this;
            EndpointID = 0;
            Controller = controller;
        }

        public Endpoint(byte endpointID, Node node)
        {
            Node = node;
            EndpointID = endpointID;
            Controller = node.Controller;
        }

        public CommandClassBase GetCommandClass(Type interfaceType)
        {
            if (interfaceType == typeof(IBasic))
                return new Basic(this);

            return null;
        }

        public override string ToString()
        {
            return $"{Node:D3}.{EndpointID:D3}";
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
                   Node == other.Node &&
                   EndpointID == other.EndpointID;
        }

        public override int GetHashCode()
        {
            var hashCode = 1833194505;
            hashCode = hashCode * -1521134295 + Node.GetHashCode();
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
