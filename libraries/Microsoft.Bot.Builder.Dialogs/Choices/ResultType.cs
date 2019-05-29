// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Controls the way that the result from `ChoicePrompt` is returned.
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// Return the value of the FoundChoice
        /// </summary>
        Value,

        /// <summary>
        /// Return the index of the FoundChoice
        /// </summary>
        Index,

        /// <summary>
        /// Returns a FoundChoice object, containing value, index, score and
        /// matched synonym.
        /// </summary>
        FoundChoice,
    }
}
