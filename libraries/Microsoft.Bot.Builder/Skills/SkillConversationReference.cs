// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// A conversation reference type for skills.
    /// </summary>
    public class SkillConversationReference
    {
        /// <summary>
        /// Gets or sets the skill conversation id.
        /// </summary>
        /// <value>This id is used to lookup, save and delete the SkillConversationReference.</value>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the conversation reference of the conversation that invoked the skill.
        /// </summary>
        /// <value>
        /// The original conversation reference.
        /// </value>
        [JsonProperty("conversationReference")]
        public ConversationReference ConversationReference { get; set; }

        /// <summary>
        /// Gets or sets the OAuth scope.
        /// </summary>
        /// <value>
        /// The OAuth scope.
        /// </value>
        public string OAuthScope { get; set; }
    }
}
