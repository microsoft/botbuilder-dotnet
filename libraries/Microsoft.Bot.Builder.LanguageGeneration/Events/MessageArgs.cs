// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Provide message data.
    /// </summary>
    public class MessageArgs : LGEventArgs
    {
        /// <summary>
        /// Gets or sets message content.
        /// </summary>
        /// <value>
        /// Message content.
        /// </value>
        public string Text { get; set; }
    }
}
