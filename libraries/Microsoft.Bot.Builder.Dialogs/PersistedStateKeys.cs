// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// These are the keys which are persisted.
    /// </summary>
    public class PersistedStateKeys : IEnumerable<string>
    {
        public string UserState { get; set; }

        public string ConversationState { get; set; }

        public IEnumerator<string> GetEnumerator()
        {
            yield return UserState;
            yield return ConversationState;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return UserState;
            yield return ConversationState;
        }
    }
}
