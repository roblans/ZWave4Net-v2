using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.Concurrent;

namespace ZWave4Net
{
    public class EndpointCollection : IEnumerable<IEndpoint>
    {
        private ConcurrentDictionary<byte, IEndpoint> _endpoints = new ConcurrentDictionary<byte, IEndpoint>();
        public readonly Node Node;

        public EndpointCollection(Node node)
        {
            Node = node;

            _endpoints.TryAdd(0, Node);
        }

        public IEnumerator<IEndpoint> GetEnumerator()
        {
            for (byte i = 0; i < 128; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEndpoint this[byte endpointID]
        {
            get { return _endpoints.GetOrAdd(endpointID, EndpointFactory.CreateEndpoint(endpointID, Node)); }
        }

    }
}
