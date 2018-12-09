using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel;

namespace ZWave4Net
{
    public static partial class Extentions
    {
        public static T ReadObject<T>(this PayloadReader reader) where T: IPayload, new()
        {
            var instance = new T();
            instance.Read(reader);
            return instance;
        }

        public static void WriteObject<T>(this PayloadWriter writer, T value) where T : IPayload
        {
            value.Write(writer);
        }

        public static T Deserialize<T>(this ByteArray payload) where T : IPayload, new()
        {
            using (var reader = new PayloadReader(payload))
            {
                return reader.ReadObject<T>();
            }
        }

        public static ByteArray Serialize(this IPayload payload)
        {
            using (var writer = new PayloadWriter())
            {
                writer.WriteObject(payload);
                return writer.ToByteArray();
            }
        }
    }
}
