// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// The SkillConversationReference is a record which is used to track a conversation with a skill.
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

        [JsonProperty("oAuthScope")]
        public string OAuthScope { get; set; }

        /// <summary>
        /// Gets or sets the EndOfConversationActivity.
        /// </summary>
        /// <value>
        /// The EndOfConversation activity which the skill sent back to the skill host to indicate the end of the conversation result.
        /// </value>
        [JsonProperty("endOfConversationActivity")]
        public Activity Activity { get; set; } = null;
    }
}
