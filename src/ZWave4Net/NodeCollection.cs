using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace ZWave
{
    /// <summary>
    ///  Collection of nodes for a controller
    /// </summary>
    public class NodeCollection : IEnumerable<Node>
    {
        private readonly List<Node> _nodes = new List<Node>();

        internal void Add(Node node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            _nodes.Add(node);
        }

        public IEnumerator<Node> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the Node for the specified ID.
        /// </summary>
        /// <param name="nodeID">The ID of the node to get</param>
        /// <returns>The node, or NULL if the Node doesn't exists</returns>
        public Node this[byte nodeID]
        {
            get { return _nodes.FirstOrDefault(element => element.NodeID == nodeID); }
        }

    }
}
