// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Schema.Converters
{
    /// <summary>
    /// Converter which allows a MemoryStream instance to be used during JSON serialization/deserialization.
    /// </summary>
#pragma warning disable CA1812 // Avoid uninstantiated internal classes.
    internal class AttachmentMemoryStreamConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(MemoryStream).IsAssignableFrom(objectType);
        }

        /// <returns>
        ///     If the object is of type:<br/>
        ///     <list type="table">
        ///         <item>
        ///             <b>List/Array</b>
        ///             <list type="bullet">
        ///                 <item><i>Without MemoryStream</i>: it will return a JArray.</item>
        ///                 <item><i>With MemoryStream</i>: it will return a List.</item>
        ///             </list>            
        ///         </item>
        ///         <item>
        ///             <b>Dictionary/Object</b>
        ///             <list type="bullet">
        ///                 <item><i>Without MemoryStream</i>: it will return a JObject.</item>
        ///                 <item><i>With MemoryStream</i>: it will return a Dictionary.</item>
        ///             </list>            
        ///         </item>
        ///     </list>
        /// </returns>
        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return JValue.CreateNull();
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                var list = new List<object>();
                reader.Read();
                while (reader.TokenType != JsonToken.EndArray)
                {
                    var item = ReadJson(reader, objectType, existingValue, serializer);
                    list.Add(item);
                    reader.Read();
                }

                if (HaveStreams(list))
                {
                    return list;
                }
                else
                {
                    return JArray.FromObject(list);
                }
            }

            if (reader.TokenType == JsonToken.StartObject)
            {
                var deserialized = serializer.Deserialize<JToken>(reader);

                var isStream = deserialized.Type == JTokenType.Object && deserialized.Value<string>("$type") == nameof(MemoryStream);
                if (isStream)
                {
                    var stream = deserialized.ToObject<SerializedMemoryStream>();
                    return new MemoryStream(stream.Buffer.ToArray());
                }

                var newReader = deserialized.CreateReader();
                newReader.Read();
                string key = null;
                var dict = new Dictionary<string, object>();
                while (newReader.Read())
                {
                    if (newReader.TokenType == JsonToken.EndObject)
                    {
                        continue;
                    }

                    if (newReader.TokenType == JsonToken.PropertyName)
                    {
                        key = newReader.Value.ToString();
                        continue;
                    }

                    var item = ReadJson(newReader, objectType, existingValue, serializer);
                    dict.Add(key, item);
                }

                var list = dict.Values.ToList();
                if (HaveStreams(list))
                {
                    return dict;
                }
                else
                {
                    return JObject.FromObject(dict);
                }
            }

            return serializer.Deserialize(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!typeof(MemoryStream).IsAssignableFrom(value.GetType()))
            {
                if (value.GetType().GetInterface(nameof(IEnumerable)) != null)
                {
                    // This makes the WriteJson loops over nested values to replace all instances of MemoryStream.
                    serializer.Converters.Add(this);
                }

                JToken.FromObject(value, serializer).WriteTo(writer);
                serializer.Converters.Remove(this);
                return;
            }

            var buffer = (value as MemoryStream).ToArray();
            var result = new SerializedMemoryStream
            {
                Type = nameof(MemoryStream),
                Buffer = buffer.ToList()
            };

            JToken.FromObject(result).WriteTo(writer);
        }

        /// <summary>
        /// Check if a List contains at least one MemoryStream.
        /// </summary>
        /// <param name="list">List of values that might have a MemoryStream instance.</param>
        /// <returns>True if there is at least one MemoryStream in the list, otherwise false.</returns>
        private static bool HaveStreams(List<object> list)
        {
            var result = false;
            foreach (var nextLevel in list)
            {
                if (nextLevel == null)
                {
                    continue;
                }

                if (nextLevel.GetType() == typeof(MemoryStream))
                {
                    result = true;
                }

                // Type generated from the ReadJson => JsonToken.StartObject.
                if (nextLevel.GetType() == typeof(Dictionary<string, object>))
                {
                    result = HaveStreams((nextLevel as Dictionary<string, object>).Values.ToList());
                }

                // Type generated from the ReadJson => JsonToken.StartArray.
                if (nextLevel.GetType() == typeof(List<object>))
                {
                    result = HaveStreams(nextLevel as List<object>);
                }

                if (result)
                {
                    break;
                }
            }

            return result;
        }

        internal class SerializedMemoryStream
        {
            [JsonProperty("$type")]
            public string Type { get; set; }

            [JsonProperty("buffer")]
            public List<byte> Buffer { get; set; }
        }
    }
#pragma warning restore CA1812
}
