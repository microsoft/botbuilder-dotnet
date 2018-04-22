// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Defines options for the LUIS recognizer.
    /// </summary>
    public class LuisRecognizerOptions : ILuisRecognizerOptions
    {
        /// <summary>
        /// Indicates whether to add metadata to the recognizer's results.
        /// </summary>
        public bool Verbose { get; set; }
    }
}
