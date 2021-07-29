// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// The importance of this recognition hint.
    /// </summary>
    public enum RecognitionHintImportance 
    {
        /// <summary>
        /// Hint corresponds to something that could be said.
        /// </summary>
        Possible,

        /// <summary>
        /// Hint corresponds to something likely to be said.
        /// </summary>
        Expected
    }
}
