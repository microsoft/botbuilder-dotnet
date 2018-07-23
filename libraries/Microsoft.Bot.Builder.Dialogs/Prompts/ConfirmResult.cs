// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    public class ConfirmResult : PromptResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the input bool recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        /// <value>
        /// The input bool recognized; or <c>null</c>, if recognition fails.
        /// </value>
        public bool Confirmation
        {
            get { return GetProperty<bool>(nameof(Confirmation)); }
            set { this[nameof(Confirmation)] = value; }
        }

        /// <summary>
        /// Gets or sets the input text recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        /// <value>
        /// The input text recognized; or <c>null</c>, if recognition fails.
        /// </value>
        public string Text
        {
            get { return GetProperty<string>(nameof(Text)); }
            set { this[nameof(Text)] = value; }
        }
    }
}
