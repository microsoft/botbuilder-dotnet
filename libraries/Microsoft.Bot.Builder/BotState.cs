// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Middleware;

namespace Microsoft.Bot.Builder
{
    public class BotState : FlexObject
    {
        public ConversationState Conversation { get; set; } = new ConversationState();
        public UserState User { get; set; } = new UserState();
    }
}
