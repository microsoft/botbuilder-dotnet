// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Bridge;

namespace Multiple_Dialogs_Bridge
{
    public class MultipleDialogsAccessors
    {
        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }
        public IStatePropertyAccessor<DictionaryDataBag> UserState { get; set; }
        public IStatePropertyAccessor<DictionaryDataBag> ConversationState { get; set; }
        public IStatePropertyAccessor<DictionaryDataBag> PrivateConversationState { get; set; }

    }
}
