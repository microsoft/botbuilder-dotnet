// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Represents recognition result for the ChoicePrompt.
    /// </summary>
    public class ChoiceResult : PromptResult
    {
        /// <summary>
        /// Gets or sets the value recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        /// <value>
        /// The value recognized; or <c>null</c>, if recognition fails.
        /// </value>
        public FoundChoice Value
        {
            get { return GetProperty<FoundChoice>(nameof(Value)); }
            set { this[nameof(Value)] = value; }
        }
    }
}
