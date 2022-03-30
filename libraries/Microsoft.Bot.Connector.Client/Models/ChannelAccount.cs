// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Microsoft.Bot.Connector.Client.Models
{
    /// <summary>
    /// Channel account information needed to route a message.
    /// </summary>
    public partial class ChannelAccount
    {
        /// <summary>
        /// Gets properties that are not otherwise defined by the <see cref="ChannelAccount"/> type but that
        /// might appear in the REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> Properties { get; } = new Dictionary<string, JsonElement>();
    }
}
