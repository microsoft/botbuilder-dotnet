// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    public class PromptRecognizerResult<T>
    {
        public PromptRecognizerResult()
        {
            Succeeded = false;
        }

        public bool Succeeded { get; set; }

        public T Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether flag indicating whether or not parent dialogs should be allowed to interrupt the prompt.
        /// </summary>
        /// <value>
        /// The default value is `false`.
        /// </value>
        public bool AllowInterruption { get; set; } = false;
    }
}
