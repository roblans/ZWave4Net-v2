using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel
{
    internal static class CommandFactory
    {
        public static Command CreateCommand(Payload payload)
        {
            if (payload.Length < 2)
                throw new ArgumentOutOfRangeException(nameof(payload), "payload must have at least 2 bytes");

            var classID = payload[0];
            var commandID = payload[1];

            using (var reader = new PayloadReader(payload))
            {
                switch (classID)
                {
                    case Crc16EndcapCommand.EncapClassID when Crc16EndcapCommand.EncapCommandID == commandID:
                        return reader.ReadObject<Crc16EndcapCommand>();
                    case MultiChannelEndcapCommand.EncapClassID when MultiChannelEndcapCommand.EncapCommandID == commandID:
                        return reader.ReadObject<MultiChannelEndcapCommand>();
                    default:
                        return reader.ReadObject<Command>();
                }
            }
        }
    }
}
