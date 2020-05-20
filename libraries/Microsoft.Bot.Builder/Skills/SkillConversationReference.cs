// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
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
        /// Gets the contextual activities.
        /// </summary>
        /// <value>
        /// For async post back we collect Event and EndOfConversation activities to be processed on SkillHost calling context.
        /// </value>
        [JsonProperty("activities")]
        public List<Activity> Activities { get; } = new List<Activity>();

        /// <summary>
        /// Gets or sets a value indicating whether a skill host is waiting on the skill to complete it's turn.
        /// </summary>
        /// <value>If this is true there is a skillHost waiting on the skill to complete the turn, activities should.</value>
        [JsonProperty("skillHostWaiting")]
        public bool SkillHostWaiting { get; set; }
    }
}
