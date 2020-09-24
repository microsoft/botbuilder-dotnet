// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Contains recognition result information.
    /// </summary>
    /// <typeparam name="T">The type of object recognized.</typeparam>
    public class ModelResult<T>
    {
        /// <summary>
        /// Gets or sets the substring of the input that was recognized.
        /// </summary>
        /// <value>The substring of the input that was recognized.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the start character position of the recognized substring.
        /// </summary>
        /// <value>The start character position of the recognized substring.</value>
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the end character position of the recognized substring.
        /// </summary>
        /// <value>The end character position of the recognized substring.</value>
        public int End { get; set; }

        /// <summary>
        /// Gets or sets the type of entity that was recognized.
        /// </summary>
        /// <value>The type of entity that was recognized.</value>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the recognized object.
        /// </summary>
        /// <value>The recognized object.</value>
        public T Resolution { get; set; }
    }
}
