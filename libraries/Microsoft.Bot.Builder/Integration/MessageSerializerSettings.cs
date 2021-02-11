// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Connector;
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
            using (var connector = new ConnectorClient(new Uri("http://localhost/")))
            {
                return connector.DeserializationSettings;
            }
        }
    }
}
