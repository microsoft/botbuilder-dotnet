// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class StoredBotState
    {
        public Dictionary<string, object> UserState { get; set; }
        public Dictionary<string, object> ConversationState { get; set; }
        public List<DialogInstance> DialogStack { get; set; }
    }
}
