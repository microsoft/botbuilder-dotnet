// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Conversation is ending, or a request to end the conversation
    /// </summary>
    public interface IEndOfConversationActivity : IActivity
    {
        /// <summary>
        /// Code indicating why the conversation has ended
        /// </summary>
        string Code { get; set; }

        /// <summary>
        /// Content to display when ending the conversation
        /// </summary>
        string Text { get; set; }
    }
}
