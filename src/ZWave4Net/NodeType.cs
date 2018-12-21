using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    /// <summary>
    /// Represents the type of a node
    /// </summary>
    public class NodeType
    {
        /// <summary>
        /// The basic type of the node
        /// </summary>
        public readonly BasicType Basic;
        /// <summary>
        /// The generic type of the node
        /// </summary>
        public readonly GenericType Generic;
        /// <summary>
        /// The specifick type of the node
        /// </summary>
        public readonly SpecificType Specific;

        internal NodeType(BasicType basic, GenericType generic, SpecificType specific)
        {
            Basic = basic;
            Generic = generic;
            Specific = specific;
        }

        public override string ToString()
        {
            return $"Basic = {Basic}, Generic = {Generic}, Specific = {Specific}";
        }
    }
}
