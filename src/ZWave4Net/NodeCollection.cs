using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace ZWave4Net
{
    public class NodeCollection : IEnumerable<Node>
    {
        private readonly List<Node> _nodes = new List<Node>();

        internal void Add(Node node)
        {
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

        public Node this[byte nodeID]
        {
            get { return _nodes.FirstOrDefault(element => element.Address.NodeID == nodeID); }
        }

    }
}
