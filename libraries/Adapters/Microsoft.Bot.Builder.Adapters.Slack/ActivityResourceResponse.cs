// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilder.Adapters.Slack
{
    /// <summary>
    /// Extends ResourceResponse with ActivityID and Conversation properties.
    /// </summary>
    public class ActivityResourceResponse : ResourceResponse
    {
        /// <summary>
        /// Gets or sets the Activity ID.
        /// </summary>
        /// <value>
        /// The Activity ID.
        /// </value>
        public string ActivityID { get; set; }

        /// <summary>
        /// Gets or sets a Conversation Account.
        /// </summary>
        /// <value>
        /// Conversation Account.
        /// </value>
        public ConversationAccount Conversation { get; set; }
    }
}
