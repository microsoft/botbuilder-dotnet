// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class StoredBotState
    {
        public IDictionary<string, object> UserState { get; set; }
        public IDictionary<string, object> ConversationState { get; set; }
        public IList<DialogInstance> DialogStack { get; set; }
    }
}
