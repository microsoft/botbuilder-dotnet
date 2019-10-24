// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// ConversationMemoryScope represents Conversation scoped memory. 
    /// </summary>
    /// <remarks>This relies on the ConversationState object being accessible from turnContext.TurnState.Get&lt;ConversationState&gt().</remarks>
    public class ConversationMemoryScope : BotStateMemoryScope<ConversationState>
    {
        public ConversationMemoryScope()
            : base(ScopePath.CONVERSATION)
        {
        }
    }
}
