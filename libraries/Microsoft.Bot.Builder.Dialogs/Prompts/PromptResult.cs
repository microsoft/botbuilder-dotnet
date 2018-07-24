// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Represents the prompt recognition result.
    /// </summary>
    /// <seealso cref="PromptStatus"/>
    /// <seealso cref="BasePromptInternal{T}"/>
    public class PromptResult : Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PromptResult"/> class.
        /// with a default <see cref="Status"/> of <see cref="PromptStatus.NotRecognized"/>.
        /// </summary>
        public PromptResult() => Status = PromptStatus.NotRecognized;

        /// <summary>
        /// Gets or sets the recognition result status.
        /// </summary>
        /// <value>
        /// The recognition result status.
        /// </value>
        public string Status { get; set; }

        /// <summary>
        /// Indicates whether the input was recognized and validated.
        /// </summary>
        /// <returns>True if the input was recognized and validated.</returns>
        public bool Succeeded() => Status == PromptStatus.Recognized;

        /// <summary>
        /// Used by derived classes to implement nullable property access this
        /// mimics the JavaScript implementation's use of undefined in some places.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>the property or null.</returns>
        protected T GetProperty<T>(string propertyName)
        {
            if (ContainsKey(propertyName))
            {
                return (T)this[propertyName];
            }

            return default(T);
        }
    }
}
