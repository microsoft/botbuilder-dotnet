// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Ai.Luis
{
    /// <summary>
    /// Options for the LUIS Recognizer.
    /// </summary>
    public interface ILuisRecognizerOptions
    {
        /// <summary>
        /// Gets a value indicating whether if set to true, metadata is added to the recognizer's results.
        /// </summary>
        bool Verbose { get; }
    }
}
