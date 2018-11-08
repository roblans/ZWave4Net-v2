using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    /// <summary>
    /// Base class for all frame types
    /// </summary>
    public class Frame : IEquatable<Frame>
    {
        private byte[] _payload;

        /// <summary>
        /// The ACK frame indicates that the receiving end received a valid Data frame
        /// </summary>
        public static readonly Frame ACK = new Frame(FrameHeader.ACK);
        
        /// <summary>
        /// The NAK frame indicates that the receiving end received a Data frame with errors
        /// </summary>
        public static readonly Frame NAK = new Frame(FrameHeader.NAK);

        /// <summary>
        /// The CAN frame indicates that the receiving end discarded an otherwise valid Data frame. 
        /// The CAN frame is used to resolve race conditions, where both ends send a Data frame and subsequently expects an ACK frame from the other end
        /// </summary>
        public static readonly Frame CAN = new Frame(FrameHeader.CAN);

        /// <summary>
        /// The SOF frame contains the Serial API command including parameters for the command in question
        /// </summary>
        public static readonly Frame SOF = new Frame(FrameHeader.SOF);

        /// <summary>
        /// The header of the frame
        /// </summary>
        public readonly FrameHeader Header;

        public Frame(FrameHeader header)
        {
            Header = header;
        }

        protected virtual byte[] GetPayload()
        {
            return new byte[] { (byte)Header };
        }

        public byte[] Payload
        {
            get { return _payload ?? (_payload = GetPayload()); }
        }

        public override string ToString()
        {
            return $"{Header}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Frame);
        }

        public bool Equals(Frame other)
        {
            return other != null && GetType() == other.GetType() && Header == other.Header;
        }

        public override int GetHashCode()
        {
            return -1422453376 + Header.GetHashCode();
        }

        public static bool operator ==(Frame frame1, Frame frame2)
        {
            return EqualityComparer<Frame>.Default.Equals(frame1, frame2);
        }

        public static bool operator !=(Frame frame1, Frame frame2)
        {
            return !(frame1 == frame2);
        }
    }
}
