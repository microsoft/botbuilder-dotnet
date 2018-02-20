// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Middleware;

namespace Microsoft.Bot.Builder
{
    public class BotState : FlexObject
    {
        public ConversationState ConversationProperties { get; set; } = new ConversationState();
        public UserState UserProperties { get; set; } = new UserState();
    }
}
