// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Represents recognition result for the NumberPrompt.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="NumberResult{T}"/>.<//typeparam>
    public class NumberResult<T> : PromptResult
    {
        /// <summary>
        /// Gets or sets the value recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        /// <value>
        /// The value recognized; or <c>null</c>, if recognition fails.
        /// </value>
        public T Value
        {
            get { return GetProperty<T>(nameof(Value)); }
            set { this[nameof(Value)] = value; }
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
