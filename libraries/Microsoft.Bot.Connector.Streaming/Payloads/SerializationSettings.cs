// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Connector.Streaming.Payloads
{
    /// <summary>
    /// This class defines the settings used when serializing data contained by objects
    /// included as part of the Bot Framework Protocol v3 with Streaming Extensions.
    /// </summary>
    public static class SerializationSettings
    {
        /// <summary>
        /// The value that should be used as the content-type header for application json.
        /// </summary>
        public const string ApplicationJson = "application/json";

        /// <summary>
        /// The serialization settings for use when operating on objects defined within the bot schema.
        /// </summary>
        public static readonly JsonSerializerSettings BotSchemaSerializationSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        /// <summary>
        /// The default serialization settings for use in most cases.
        /// </summary>
        public static readonly JsonSerializerSettings DefaultSerializationSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        };

        /// <summary>
        /// The default deserialization settings for use in most cases.
        /// </summary>
        public static readonly JsonSerializerSettings DefaultDeserializationSettings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        };
    }
}
