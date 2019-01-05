using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public class MultiChannelEndpointsReport : Report
    {
        public bool IsDynamicNumberOfEndpoints { get; private set; }

        public bool AllEndpointsAreIdentical { get; private set; }

        public byte NumberOfIndividualEndpoints { get; private set; }

        public byte NumberOfAggregatedEndpoints { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            var endpointsInfo = reader.ReadByte();

            IsDynamicNumberOfEndpoints = (endpointsInfo & 0x80) > 0;
            AllEndpointsAreIdentical = (endpointsInfo & 0x40) > 0;
            NumberOfIndividualEndpoints = (byte)(reader.ReadByte() & 0x7F);

            // For version 4 only.
            if (reader.Position < reader.Length)
            {
                NumberOfAggregatedEndpoints = (byte)(reader.ReadByte() & 0x7F);
            }
        }

        public override string ToString()
        {
            return $"IsDynamicNumberOfEndpoints: {IsDynamicNumberOfEndpoints}, AllEndpointsAreIdentical: {AllEndpointsAreIdentical}, NumberOfIndividualEndpoints: {NumberOfIndividualEndpoints}, NumberOfAggregatedEndpoints: {NumberOfAggregatedEndpoints}";
        }
    }
}
