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

        public DialogState(IList<DialogInstance> stack)
        {
            DialogStack = stack ?? new List<DialogInstance>();
            ConversationState = new Dictionary<string, object>();
            UserState = new Dictionary<string, object>();
        }

        public IList<DialogInstance> DialogStack { get; set; } = new List<DialogInstance>();

        public IDictionary<string, object> ConversationState { get; set; } = new Dictionary<string, object>();

        public IDictionary<string, object> UserState { get; set; } = new Dictionary<string, object>();
    }
}
