// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>Channel account information needed to route a message.</summary>
    public class ChannelAccount
    {
        /// <summary>Initializes a new instance of the <see cref="ChannelAccount"/> class.</summary>
        public ChannelAccount()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ChannelAccount"/> class.</summary>
        /// <param name="id">Channel id for the user or bot on this channel (Example: joe@smith.com, or @joesmith or 123456).</param>
        /// <param name="name">Display friendly name.</param>
        /// <param name="aadObjectId">This account's object ID within Azure Active Directory (AAD).</param>
        /// <param name="role">Role of the entity behind the account (Example:User, Bot, etc.). Possible values include: 'user', 'bot'.</param>
        public ChannelAccount(string id = default, string name = default, string role = default, string aadObjectId = default)
        {
            Id = id;
            Name = name;
            AadObjectId = aadObjectId;
            Role = role;
        }

        /// <summary>Gets or sets channel id for the user or bot on this channel (Example: joe@smith.com, or @joesmith or 123456).</summary>
        /// <value>The channel ID for the user or bot.</value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>Gets or sets display friendly name.</summary>
        /// <value>The friendly display name.</value>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>Gets or sets this account's object ID within Azure Active Directory (AAD).</summary>
        /// <value>The account's object ID within Azure Active Directory.</value>
        [JsonPropertyName("aadObjectId")]
        public string AadObjectId { get; set; }

        /// <summary>
        /// Gets or sets properties that are not otherwise defined by the <see cref="ChannelAccount"/> type but that
        /// might appear in the REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonExtensionData]
        public Dictionary<string, JsonElement> Properties { get; set; } = new Dictionary<string, JsonElement>();

        /// <summary>Gets or sets role of the entity behind the account (Example: User, Bot, etc.). Possible values include: 'user', 'bot'.</summary>
        /// <value>The role of the entity behind the account.</value>
        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}
