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
        /// The value recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public FoundChoice Value
        {
            get { return GetProperty<FoundChoice>(nameof(Value)); }
            set { this[nameof(Value)] = value; }
        }
    }
}