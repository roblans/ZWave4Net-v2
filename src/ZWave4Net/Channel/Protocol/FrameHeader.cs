using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    public enum FrameHeader : byte
    {
        /// <summary>
        /// The SOF frame contains the Serial API command including parameters for the command in question
        /// </summary>
        SOF = 0x01,
        
        /// <summary>
        /// The ACK frame indicates that the receiving end received a valid Data frame
        /// </summary>
        ACK = 0x06,

        /// <summary>
        /// The NAK frame indicates that the receiving end received a Data frame with errors
        /// </summary>
        NAK = 0x15,

        /// <summary>
        /// The CAN frame indicates that the receiving end discarded an otherwise valid Data frame. 
        /// The CAN frame is used to resolve race conditions, where both ends send a Data frame and subsequently expects an ACK frame from the other end
        /// </summary>
        CAN = 0x18
    }
}
