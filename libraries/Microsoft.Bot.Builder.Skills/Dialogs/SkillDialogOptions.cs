// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Skills.Dialogs
{
    public class SkillDialogOptions
    {
        public string BotId { get; set; }

        public BotFrameworkClient SkillClient { get; set; }

        public Uri SkillHostEndpoint { get; set; }

        public SkillConversationIdFactoryBase ConversationIdFactory { get; set; }
    }
}
