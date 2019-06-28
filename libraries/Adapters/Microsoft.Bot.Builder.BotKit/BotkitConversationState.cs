// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.BotKit
{
    /// <summary>
    ///  A customized version of ConversationState that override the getStorageKey method to create a more complex key value.
    ///  This allows Botkit to automatically track conversation state in scenarios where multiple users are present in a single channel,
    ///  or when threads or sub-channels parent channel that would normally collide based on the information defined in the conversation address field.
    ///  Note: This is used automatically inside Botkit and developers should not need to directly interact with it.
    /// </summary>
    public class BotkitConversationState : ConversationState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotkitConversationState"/> class.
        /// </summary>
        /// <param name="storage">storage of the BotkitConversationState.</param>
        public BotkitConversationState(IStorage storage)
            : base(storage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotkitConversationState"/> class.
        /// </summary>
        /// <param name="context">TurnContext context.</param>
        /// <returns>string type returned.</returns>
        public string GetStorageKey(TurnContext context)
        {
            Activity activity = context.Activity;
            string channelId = activity.ChannelId;

            if (activity.Conversation == null || activity.Conversation.Id == null)
            {
                throw new Exception("missing activity.conversation");
            }

            // create a combo key by sorting all the fields in the conversation address and combining them all
            // mix in user id as well, because conversations are between the bot and a single user
            const string ConversationId = "";

            if (channelId == null)
            {
                throw new Exception("missing activity.channelId");
            }

            if (ConversationId == null)
            {
                throw new Exception("missing activity.conversation.id");
            }

            return $"{channelId}/ conversations /{ConversationId}/{typeof(BotkitConversationState).Namespace}";
        }
    }
}
