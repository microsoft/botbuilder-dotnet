// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Channel account information needed to route a message
    /// </summary>
    public partial class ChannelAccount
    {
        /// <summary>
        /// Initializes a new instance of the ChannelAccount class.
        /// </summary>
        public ChannelAccount()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ChannelAccount class.
        /// </summary>
        /// <param name="id">Channel id for the user or bot on this channel
        /// (Example: joe@smith.com, or @joesmith or 123456)</param>
        /// <param name="name">Display friendly name</param>
        /// <param name="aadObjectId">This account's object ID within Azure
        /// Active Directory (AAD)</param>
        /// <param name="role">Role of the entity behind the account (Example:
        /// User, Bot, etc.). Possible values include: 'user', 'bot'</param>
        public ChannelAccount(string id = default(string), string name = default(string), string role = default(string), string aadObjectId = default(string))
        {
            Id = id;
            Name = name;
            AadObjectId = aadObjectId;
            Role = role;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets channel id for the user or bot on this channel
        /// (Example: joe@smith.com, or @joesmith or 123456)
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets display friendly name
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets this account's object ID within Azure Active Directory
        /// (AAD)
        /// </summary>
        [JsonProperty(PropertyName = "aadObjectId")]
        public string AadObjectId { get; set; }

        /// <summary>
        /// Gets or sets role of the entity behind the account (Example: User,
        /// Bot, etc.). Possible values include: 'user', 'bot'
        /// </summary>
        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }

    }
}
