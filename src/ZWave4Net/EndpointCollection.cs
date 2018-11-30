using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.Concurrent;

namespace ZWave4Net
{
    public class EndpointCollection : IEnumerable<Endpoint>
    {
        private ConcurrentDictionary<byte, Endpoint> _endpoints = new ConcurrentDictionary<byte, Endpoint>();
        public readonly Node Node;

        public EndpointCollection(Node node)
        {
            Node = node;

            _endpoints.TryAdd(0, Node);
        }

        public IEnumerator<Endpoint> GetEnumerator()
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

        public Endpoint this[byte endpointID]
        {
            get { return _endpoints.GetOrAdd(endpointID, new Endpoint(endpointID, Node, Node.Controller)); }
        }

    }
}
