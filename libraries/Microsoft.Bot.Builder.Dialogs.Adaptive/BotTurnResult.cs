// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class BotTurnResult
    {
        public DialogTurnResult TurnResult { get; set; }
        public List<Activity> Activities { get; set; }
        public StoredBotState NewState { get; set; }
    }
}
