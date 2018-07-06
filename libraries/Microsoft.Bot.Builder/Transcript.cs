// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Represents a copy of a conversation.
    /// </summary>
    public class Transcript
    {
        /// <summary>
        /// Gets or sets the ID of the channel in which the conversation occurred.
        /// </summary>
        /// <value>The ID of the channel in which the conversation occurred.</value>
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the conversation.
        /// </summary>
        /// <value>The ID of the conversation.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the date the conversation began.
        /// </summary>
        /// <value>The date then conversation began.</value>
        public DateTimeOffset Created { get; set; }
    }
}
