using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave.Channel.Protocol.Frames
{
    public enum DataFrameType : byte
    {
        /// <summary>
        /// Request. This type MUST be used for unsolicited messages. API callback messages MUST use the Request type.
        /// </summary>
        REQ = 0x00,

        /// <summary>
        /// Response. This type MUST be used for messages that are responses to Requests.
        /// </summary>
        RES = 0x01,
    }
}
