// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogManagerResult
    {
        public DialogTurnResult TurnResult { get; set; }

        public Activity[] Activities { get; set; }

        public PersistedState NewState { get; set; }
    }
}
