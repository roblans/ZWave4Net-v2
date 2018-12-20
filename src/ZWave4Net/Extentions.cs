using System;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.Channel;

namespace ZWave4Net
{
    internal static partial class Extentions
    {
        public static T ReadObject<T>(this PayloadReader reader) where T: IPayloadSerializable, new()
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var instance = new T();
            instance.Read(reader);
            return instance;
        }

        internal static SpecificType ReadSpecificType(this PayloadReader reader, GenericType genericType)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var specificType = reader.ReadByte();
            if (specificType == 0)
            {
                return SpecificType.NotUsed;
            }
            else
            {
                return (SpecificType)((int)genericType << 16 | specificType);
            }
        }
        public static void WriteObject<T>(this PayloadWriter writer, T value) where T : IPayloadSerializable
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            value.Write(writer);
        }

        public static T Deserialize<T>(this Payload payload) where T : IPayloadSerializable, new()
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            using (var reader = new PayloadReader(payload))
            {
                return reader.ReadObject<T>();
            }
        }

        public static Payload Serialize(this IPayloadSerializable serializable)
        {
            if (serializable == null)
                throw new ArgumentNullException(nameof(serializable));

            using (var writer = new PayloadWriter())
            {
                writer.WriteObject(serializable);
                return writer.ToPayload();
            }
        }
    }
}
