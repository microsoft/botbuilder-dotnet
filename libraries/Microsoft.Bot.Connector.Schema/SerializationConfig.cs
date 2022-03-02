// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// This class defines the configuration settings used when serializing and deserializing data
    /// contained in objects defined by the Bot Framework Protocol schema.
    /// </summary>
    public static class SerializationConfig
    {
        /// <summary>
        /// The default serialization settings for use in most cases.
        /// </summary>
        public static readonly JsonSerializerOptions DefaultSerializeOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true
        };

        /// <summary>
        /// The default deserialization settings for use in most cases.
        /// </summary>
        public static readonly JsonSerializerOptions DefaultDeserializeOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true
        };
    }
}
