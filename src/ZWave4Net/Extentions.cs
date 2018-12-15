using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel;

namespace ZWave4Net
{
    public static partial class Extentions
    {
        public static T ReadObject<T>(this PayloadReader reader) where T: IPayloadSerializable, new()
        {
            var instance = new T();
            instance.Read(reader);
            return instance;
        }

        public static void WriteObject<T>(this PayloadWriter writer, T value) where T : IPayloadSerializable
        {
            value.Write(writer);
        }

        public static T Deserialize<T>(this Payload payload) where T : IPayloadSerializable, new()
        {
            using (var reader = new PayloadReader(payload))
            {
                return reader.ReadObject<T>();
            }
        }

        public static Payload Serialize(this IPayloadSerializable serializable)
        {
            using (var writer = new PayloadWriter())
            {
                writer.WriteObject(serializable);
                return writer.ToPayload();
            }
        }
    }
}
