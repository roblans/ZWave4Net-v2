using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net
{
    public class NodeType
    {
        public readonly BasicType Basic;
        public readonly GenericType Generic;
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
