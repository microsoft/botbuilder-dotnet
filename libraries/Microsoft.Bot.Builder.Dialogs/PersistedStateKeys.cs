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
        /// <summary>
        /// Gets or sets the key for the user state.
        /// </summary>
        /// <value>The key for the user state.</value>
        public string UserState { get; set; }

        /// <summary>
        /// Gets or sets the key for the conversation state.
        /// </summary>
        /// <value>The key for the conversation state.</value>
        public string ConversationState { get; set; }

        /// <summary>
        /// Gets the collection of persisted state keys.
        /// </summary>
        /// <returns>A collection of persisted state keys.</returns>
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
