// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class PersistedState
    {
        public PersistedState()
        {
            this.UserState = new Dictionary<string, object>();
            this.ConversationState = new Dictionary<string, object>();
        }

        public PersistedState(PersistedStateKeys keys, IDictionary<string, object> data)
        {
            UserState = data.ContainsKey(keys.UserState) ? data[keys.UserState] as Dictionary<string, object> : new Dictionary<string, object>();
            ConversationState = data.ContainsKey(keys.ConversationState) ? data[keys.ConversationState] as Dictionary<string, object> : new Dictionary<string, object>();
        }

        public IDictionary<string, object> UserState { get; set; }

        public IDictionary<string, object> ConversationState { get; set; }
    }
}
