using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.CommandClasses
{
    /// <summary>
    /// Advertises the current destinations of a given association group
    /// </summary>
    public class AssociationReport : Report
    {
        /// <summary>
        /// This field is used to advertise the actual association group.
        /// </summary>
        public byte GroupID { get; private set; }

        /// <summary>
        /// The maximum number of destinations supported by the advertised association group. Each destination MAY be a NodeID destination or an End Point destination (if the node supports the Multi Channel Association Command Class).
        /// </summary>
        public byte MaxNodesSupported { get; private set; }

        /// <summary>
        /// The entire list destinations of the advertised association group may be too long for one command. This field advertise how many report frames will follow this report
        /// </summary>
        public byte ReportsToFollow { get; private set; }

        /// <summary>
        /// This field advertises a list of NodeID destinations of the advertised association group. The list of NodeIDs is empty if there are no NodeID destinations configured for the advertised association group.
        /// </summary>
        public byte[] Nodes { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

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
