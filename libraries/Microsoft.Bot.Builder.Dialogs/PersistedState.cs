// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Concurrent;
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
            UserState = data.ContainsKey(keys.UserState) ? (IDictionary<string, object>)data[keys.UserState] : new ConcurrentDictionary<string, object>();
            ConversationState = data.ContainsKey(keys.ConversationState) ? (IDictionary<string, object>)data[keys.ConversationState] : new ConcurrentDictionary<string, object>();
        }

        public IDictionary<string, object> UserState { get; set; }

        public IDictionary<string, object> ConversationState { get; set; }
    }
}
