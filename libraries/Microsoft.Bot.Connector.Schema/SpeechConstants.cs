// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines constants that can be used in the processing of speech interactions.
    /// </summary>
    public static class SpeechConstants
    {
        /// <summary>
        /// The xml tag structure to indicate an empty speak tag, to be used in the 'speak' property of an Activity. When set this indicates to the channel that speech should not be generated.
        /// </summary>
        public static readonly string EmptySpeakTag = "<speak version=\"1.0\" xmlns=\"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-US\" />";
    }
}
