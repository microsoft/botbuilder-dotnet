// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Handles persistence of a conversation state object using the conversation ID and From ID as part of the key.
    /// </summary>
    public class PrivateConversationState : BotState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateConversationState"/> class.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        public PrivateConversationState(IStorage storage)
            : base(storage, nameof(PrivateConversationState))
        {
        }

        /// <summary>
        /// Gets the key to use when reading and writing state to and from storage.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>The storage key.</returns>
        protected override string GetStorageKey(ITurnContext turnContext) => $"conversation/{turnContext.Activity.ChannelId}/{turnContext.Activity.Conversation.Id}/{turnContext.Activity.From.Id}";
    }
}
