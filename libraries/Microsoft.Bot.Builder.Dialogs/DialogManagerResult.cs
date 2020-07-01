// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogManagerResult
    {
        public DialogTurnResult TurnResult { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
        public Activity[] Activities { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        public PersistedState NewState { get; set; }
    }
}
