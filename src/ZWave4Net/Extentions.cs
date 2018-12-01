using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel;

namespace ZWave4Net
{
    public static partial class Extentions
    {
        public static T ReadObject<T>(this PayloadReader reader) where T: IPayloadReadable, new()
        {
            var instance = new T();
            instance.ReadFrom(reader);
            return instance;
        }

        public static void WriteObject<T>(this PayloadWriter writer, T value) where T : IPayloadWriteable
        {
            value.WriteTo(writer);
        }
    }
}
