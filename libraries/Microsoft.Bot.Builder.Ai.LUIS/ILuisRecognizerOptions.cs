// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Options for the Luis Recognizer.
    /// </summary>
    public interface ILuisRecognizerOptions
    {
        /// <summary>
        /// Gets a value indicating whether to add metadata to the recognizer's results.
        /// </summary>
        /// <value>
        /// A value indicating whether to add metadata to the recognizer's results.
        /// </value>
        bool Verbose { get; }
    }
}
