// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Channel account information needed to route a message.
    /// </summary>
    public partial class ChannelAccount
    {
        /// <summary>
        /// Gets or sets properties that are not otherwise defined by the <see cref="ChannelAccount"/> type but that
        /// might appear in the REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData(ReadData = true, WriteData = true)]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public JObject Properties { get; set; } = new JObject();
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
