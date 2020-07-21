using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    // From https://stackoverflow.com/a/33296533/12248433
    public class MemoryStreamJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(MemoryStream).IsAssignableFrom(objectType) || typeof(Headers).IsAssignableFrom(objectType) || typeof(Stream).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(MemoryStream) || objectType == typeof(Stream))
            {
                var token = JToken.Load(reader);

                var stream = new MemoryStream();
                if (!token.HasValues)
                {
                    return stream;
                }

                var byteArray = Encoding.UTF8.GetBytes(token.Value<object>().ToString());
                return new MemoryStream(byteArray);

                //var test = JToken.Load(reader);
                //var bytes = serializer.Deserialize<byte[]>(reader);
                //return bytes != null ? new MemoryStream(bytes) : new MemoryStream();
            }
            else if (objectType == typeof(Headers))
            {
                var json = JObject.Load(reader);
                var headers = new Headers();

                foreach (var header in json)
                {
                    headers.Add(header.Key, header.Value.ToString());
                }

                return headers;
            }

            return serializer.Deserialize(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() == typeof(MemoryStream) || value.GetType() == typeof(Stream))
            {
                using (var sr = new StreamReader((Stream)value))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    var x = serializer.Deserialize(jsonTextReader);
                    serializer.Serialize(writer, x);
                }

                //var valueStream = (Stream)value;
                //var fileBytes = new byte[valueStream.Length];

                //valueStream.Read(fileBytes, 0, (int)valueStream.Length);

                //var bytesAsString = Convert.ToBase64String(fileBytes);
                //serializer.Serialize(writer, (Stream)value);
            }
            else if (value.GetType() == typeof(Headers))
            {
                var json = new JObject();
                foreach (var header in value as Headers)
                {
                    json[header] = (value as Headers)?[header];
                }

                serializer.Serialize(writer, json);
            }
        }
    }
}
