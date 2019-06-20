using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Protocol
{
    public static class SerializationSettings
    {
        public const string ApplicationJson = "application/json";

        public static readonly JsonSerializerSettings BotSchemaSerializationSettings = new JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.None,
            DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            //Converters = new List<JsonConverter>
            //{
            //    new Iso8601TimeSpanConverter(),
            //},
        };

        public static readonly JsonSerializerSettings DefaultSerializationSettings = new JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.None,
            DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
            //ContractResolver = new ReadOnlyJsonContractResolver(),
            //Converters = new List<JsonConverter>
            //{
            //    new Iso8601TimeSpanConverter(),
            //},
        };

        public static readonly JsonSerializerSettings DefaultDeserializationSettings = new JsonSerializerSettings
        {
            DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
            //ContractResolver = new ReadOnlyJsonContractResolver(),
            //Converters = new List<JsonConverter>
            //    {
            //        new Iso8601TimeSpanConverter(),
            //    },
        };
    }
}
