// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Handles persistence of a conversation state object using the conversation ID as part of the key.
    /// </summary>
    public class ConversationState : BotState
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationState"/> class.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        public ConversationState(IStorage storage)
            : base(storage, nameof(ConversationState))
        {
        }

        public ConversationState(IStorage storage, int maxKeyLength)
            : base(storage, nameof(ConversationState))
        {
            // Note: There is no min check here, as different data stores have different
            // requiements.
            MaxKeyLength = maxKeyLength;
        }

        public int MaxKeyLength { get; } = 254;

        /// <summary>
        /// Gets the key to use when reading and writing state to and from storage.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>The storage key.</returns>
        protected override string GetStorageKey(ITurnContext turnContext)
        {
            var channelId = turnContext.Activity.ChannelId ?? throw new ArgumentNullException("invalid activity-missing channelId");
            var conversationId = turnContext.Activity.Conversation?.Id ?? throw new ArgumentNullException("invalid activity-missing Conversation.Id");

            var firstSegment = $"{channelId}/conversations/";
            if (firstSegment.Length + conversationId.Length > MaxKeyLength)
            {
                // Some data stores, such as CosmosDB, have a key length limitation, as seen here:
                // https://docs.microsoft.com/en-us/azure/cosmos-db/faq#table
                // Some channels, such as Teams, return long conversations Ids.

                // This code checks for an artibraty key legth, and just uses a hash of the ConversationId if
                // the length is too long.
                conversationId = conversationId.GetHashCode().ToString("x");
            }

            return $"{firstSegment}{conversationId}";
        }
    }
}
