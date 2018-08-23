// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Controls the way that choices for a `ChoicePrompt` or yes/no options for a `ConfirmPrompt` are
    /// presented to a user.
    /// </summary>
    public enum ListStyle
    {
        /// <summary>
        /// Don't include any choices for prompt.
        /// </summary>
        None,

        /// <summary>
        /// Automatically select the appropriate style for the current channel.
        /// </summary>
        Auto,

        /// <summary>
        /// Add choices to prompt as an inline list.
        /// </summary>
        Inline,

        /// <summary>
        /// Add choices to prompt as a numbered list.
        /// </summary>
        List,

        /// <summary>
        /// Add choices to prompt as suggested actions.
        /// </summary>
        SuggestedAction,
    }
}
