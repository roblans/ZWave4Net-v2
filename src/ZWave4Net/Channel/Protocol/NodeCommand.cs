using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    //public class NodeCommand : RequestDataFrame
    //{
    //    private static readonly object _lock = new object();
    //    private static byte _callbackID = 0;

    //    public readonly byte NodeID;
    //    public readonly byte[] Payload;
    //    public readonly byte CallbackID;


    //    public NodeCommand(byte nodeID, byte[] payload) 
    //        : base(ControllerFunction.SendData)
    //    {
    //        if ((NodeID = nodeID) == 0)
    //            throw new ArgumentOutOfRangeException(nameof(NodeID), nodeID, "NodeID can not be 0");
    //        if ((Payload = payload) == null)
    //            throw new ArgumentNullException(nameof(payload));

    //        CallbackID = GetNextCallbackID();
    //    }

    //    private static byte GetNextCallbackID()
    //    {
    //        lock (_lock) { return _callbackID = (byte)((_callbackID % 255) + 1); }
    //    }

    //    public override void WritePayload(BinaryWriter writer)
    //    {
    //        writer.Write(NodeID);
    //        writer.Write(Payload);
    //        writer.Write((byte)(TransmitOptions.Ack | TransmitOptions.AutoRoute | TransmitOptions.Explore));
    //        writer.Write(CallbackID);
    //    }
    //}
}
