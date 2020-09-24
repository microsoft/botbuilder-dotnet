// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// A conversation reference type for skills.
    /// </summary>
    public class SkillConversationReference
    {
        /// <summary>
        /// Gets or sets the conversation reference.
        /// </summary>
        /// <value>
        /// The conversation reference.
        /// </value>
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
