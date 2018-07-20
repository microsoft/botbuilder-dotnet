// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// A user has added a bot to their contact list, removed the bot from their contact list, or otherwise changed the relationship between user and bot
    /// </summary>
    public class ContactRelationUpdateActivity : Activity
    {
        public ContactRelationUpdateActivity() : base(ActivityTypes.ContactRelationUpdate)
        {
        }

        /// <summary>
        /// Gets or sets contactAdded/Removed action
        /// </summary>
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }
    }
}
