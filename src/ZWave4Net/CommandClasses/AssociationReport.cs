using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public class AssociationReport : Report
    {
        public byte GroupID { get; private set; }
        public byte MaxNodesSupported { get; private set; }
        public byte ReportsToFollow { get; private set; }
        public byte[] Nodes { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            GroupID = reader.ReadByte();
            MaxNodesSupported = reader.ReadByte();
            ReportsToFollow = reader.ReadByte();
            Nodes = reader.ReadBytes(reader.Length - reader.Position);
        }

        public override string ToString()
        {
            return $"GroupID: {GroupID}, Nodes: {string.Join(", ", Nodes)}";
        }
    }
}
