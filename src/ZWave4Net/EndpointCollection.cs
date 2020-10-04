using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.Concurrent;

namespace ZWave
{
    /// <summary>
    ///  Collection of endpoints for a node
    /// </summary>
    public class EndpointCollection : IEnumerable<Endpoint>
    {
        private readonly ConcurrentDictionary<byte, Endpoint> _endpoints = new ConcurrentDictionary<byte, Endpoint>();
        public readonly Node Node;

        internal EndpointCollection(Node node)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));

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

        /// <summary>
        /// Gets the Endpoint for the specified ID.
        /// </summary>
        /// <param name="endpointID">The ID of the Endpoint to get</param>
        /// <returns>The Endpoint for the specified ID</returns>
        public Endpoint this[byte endpointID]
        {
            get { return _endpoints.GetOrAdd(endpointID, (element) => Node.CreateEndpoint(element)); }
        }

    }
}
