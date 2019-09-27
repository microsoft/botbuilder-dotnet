// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    /// <summary>
    /// Extends ResourceResponse with ActivityId and Conversation properties.
    /// </summary>
    public class ActivityResourceResponse : ResourceResponse
    {
        /// <summary>
        /// Gets or sets the Activity Id.
        /// </summary>
        /// <value>
        /// The Activity Id.
        /// </value>
        public string ActivityId { get; set; }

        /// <summary>
        /// Gets or sets a Conversation Account.
        /// </summary>
        /// <value>
        /// Conversation Account.
        /// </value>
        public ConversationAccount Conversation { get; set; }
    }
}
