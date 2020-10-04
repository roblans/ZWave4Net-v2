using System;
using System.Linq;

namespace ZWave.CommandClasses
{
    public class MultiChannelAssociationReport : Report
    {
        private const byte MultiChannelAssociationReportMarker = 0;

        public byte GroupID { get; private set; }
        public byte MaxNodesSupported { get; private set; }
        public byte ReportsToFollow { get; private set; }
        public byte[] Nodes { get; private set; }
        public EndpointAssociation[] Endpoints { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            GroupID = reader.ReadByte();
            MaxNodesSupported = reader.ReadByte();
            ReportsToFollow = reader.ReadByte();

            var payload = reader.ReadBytes(reader.Length - reader.Position);
            Nodes = payload.TakeWhile(element => element != MultiChannelAssociationReportMarker).ToArray();

            var endpointsPayload = payload.SkipWhile(element => element != MultiChannelAssociationReportMarker).Skip(1).ToArray();

            Endpoints = new EndpointAssociation[endpointsPayload.Length / 2];
            for (int i = 0, j = 0; i < Endpoints.Length; i++, j += 2)
            {
                Endpoints[i] = new EndpointAssociation(endpointsPayload[j], endpointsPayload[j + 1]);
            }
        }

        public override string ToString()
        {
            return $"GroupID: {GroupID}, Nodes: {string.Join(", ", Nodes)}, Endpoints: {string.Join(", ", Endpoints.Cast<object>().ToArray())}";
        }
    }
}
