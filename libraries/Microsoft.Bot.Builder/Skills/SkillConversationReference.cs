// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Skills
{
    public class SkillConversationReference
    {
        public ConversationReference ConversationReference { get; set; }

        public string OAuthScope { get; set; }
    }
}
