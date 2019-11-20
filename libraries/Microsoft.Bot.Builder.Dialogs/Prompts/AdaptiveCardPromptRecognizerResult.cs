// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Additional items to include on PromptRecognizerResult, as necessary.
    /// </summary>
    /// <typeparam name="T">Type returned by recognizer.</typeparam>
    public class AdaptiveCardPromptRecognizerResult<T> : PromptRecognizerResult<T>
    {
        /// <summary>
        /// Gets or sets Error enum.
        /// </summary>
        /// <value>
        /// If not recognized.succeeded, include reason why, if known.
        /// </value>
        public AdaptiveCardPromptErrors Error { get; set; }

        /// <summary>
        /// Gets or sets array of missing required Ids.
        /// </summary>
        /// <value>
        /// Array of requiredIds that were not included with user input.
        /// </value>
        public List<string> MissingIds { get; set; }
    }
}
