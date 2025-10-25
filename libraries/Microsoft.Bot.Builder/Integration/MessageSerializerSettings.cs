// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration
{
    /// <summary>
    /// A class containing serializer settings for Microsoft.Bot.Connector.
    /// </summary>
    public static class MessageSerializerSettings
    {
        /// <summary>
        /// Creates a new ConnectorClient deserialization settings object.
        /// </summary>
        /// <returns>A <see cref="JsonSerializerSettings"/> object.</returns>
        public static JsonSerializerSettings Create()
        {
           return new JsonSerializerSettings
           {
               DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
               DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
               NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
               ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
               ContractResolver = new ReadOnlyJsonContractResolver(),
               Converters = new List<JsonConverter>
                    {
                        new Iso8601TimeSpanConverter()
                    },
               MaxDepth = null
           };
        }
    }
}
