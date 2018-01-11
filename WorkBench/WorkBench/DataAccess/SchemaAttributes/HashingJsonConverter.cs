using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkBench.DataAccess.SchemaAttributes
{
    public class HashingJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(CryptographicHelper.SHA256(value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string)
                || objectType == typeof(int)
                || objectType == typeof(decimal);
        }
    }
}
