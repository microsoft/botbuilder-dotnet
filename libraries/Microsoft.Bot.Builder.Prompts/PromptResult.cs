// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Prompts
{
    /// <summary>
    /// Represents the prompt recognition result.
    /// </summary>
    /// <seealso cref="PromptStatus"/>
    /// <seealso cref="BasePrompt{T}"/>
    public class PromptResult : Dictionary<string, object>
    {
        /// <summary>
        /// Creates a <see cref="PromptResult"/> object with a default
        /// <see cref="Status"/> of <see cref="PromptStatus.NotRecognized"/>.
        /// </summary>
        public PromptResult()
        {
            Status = PromptStatus.NotRecognized;
        }

        /// <summary>
        /// The recognition result status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Indicates whether the input was recognized and validated.
        /// </summary>
        /// <returns>True if the input was recognized and validated.</returns>
        public bool Succeeded() { return Status == PromptStatus.Recognized; }

        /// <summary>
        /// Used by derived classes to implement nullable property access this
        /// mimics the JavaScript implementation's use of undefined in some places
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns>the property or null</returns>
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