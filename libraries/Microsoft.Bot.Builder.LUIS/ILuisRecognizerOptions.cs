// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.LUIS
{
    /// <summary>
    /// Options for the Luis Recognizer
    /// </summary>
    public interface ILuisRecognizerOptions
    {
        /// <summary>
        /// If set to true, metadata is added to the recognizer's results
        /// </summary>
        bool Verbose { get; }
    }
}
