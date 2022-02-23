// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Contains the result returned by the recognition method of a <see cref="Prompt{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value the prompt returns.</typeparam>
    public class PromptRecognizerResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PromptRecognizerResult{T}"/> class.
        /// </summary>
        public PromptRecognizerResult()
        {
            Succeeded = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the recognition attempt succeeded.
        /// </summary>
        /// <value>True if the recognition attempt succeeded; otherwise, false.</value>
        public bool Succeeded { get; set; }

        /// <summary>
        /// Gets or sets the recognition value.
        /// </summary>
        /// <value>If <see cref="Succeeded"/> is true, the recognition result from the prompt.</value>
        public T Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether flag indicating whether or not parent dialogs should be allowed to interrupt the prompt.
        /// </summary>
        /// <value>
        /// The default value is `false`.
        /// </value>
        public bool AllowInterruption { get; set; }
    }
}
