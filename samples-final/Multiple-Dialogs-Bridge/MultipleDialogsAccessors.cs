// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Multiple_Dialogs_Bridge
{
    public class MultipleDialogsAccessors
    {
        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }
    }
}
