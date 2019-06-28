// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.BotKit
{
    /// <summary>
    /// A base class for a `bot` instance, an object that contains the information and functionality for taking action in response to an incoming message.
    /// </summary>
    public class BotWorkerConfiguration
    {
        /// <summary>
        /// Gets or Sets the DialogContext of the BotWorkerConfiguration.
        /// </summary>
        public DialogContext DialogContext { get; set; }

        /// <summary>
        /// Gets or Sets the TurnContext of the BotWorkerConfiguration.
        /// </summary>
        public TurnContext TurnContext { get; set; }

        /// <summary>
        /// Gets or Sets the ConversationReference of the BotWorkerConfiguration.
        /// </summary>
        public ConversationReference ConversationReference { get; set; }

        /// <summary>
        /// Gets or Sets the Activity of the BotWorkerConfiguration.
        /// </summary>
        public Activity Activity { get; set; }
    }
}
