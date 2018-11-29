using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
