// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogState
    {
        public DialogState()
            : this(null)
        {
        }

        public DialogState(List<DialogInstance> stack)
        {
            DialogStack = stack ?? new List<DialogInstance>();
            ConversationState = new StateMap();
            UserState = new StateMap();
        }

        public List<DialogInstance> DialogStack { get; set; }

        public StateMap ConversationState { get; set; }
        public StateMap UserState { get; set; }
    }
}
