// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Represents recognition result for the NumberPrompt.
    /// </summary>
    public class NumberResult<T> : PromptResult
    {
        /// <summary>
        /// The value recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public T Value
        {
            get { return GetProperty<T>(nameof(Value)); }
            set { this[nameof(Value)] = value; }
        }

        /// <summary>
        /// The input text recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public string Text
        {
            get { return GetProperty<string>(nameof(Text)); }
            set { this[nameof(Text)] = value; }
        }
    }
}