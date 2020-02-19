// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// ConversationReference with Audience for proactive messages to other bots.
    /// </summary>
    public class SkillConversationReference : ConversationReference
    {
        /// <summary>
        /// Gets or sets the Audience paired to this ConversationReference.
        /// </summary>
        /// <value>
        /// The audience value the skill will use when forwarding activities to other bots.
        /// </value>
        public string Audience { get; set; }
    }
}
