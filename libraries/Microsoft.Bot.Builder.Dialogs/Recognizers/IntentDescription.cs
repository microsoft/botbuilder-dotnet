// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.Bot.Builder.Dialogs.Recognizers
{
    /// <summary>
    /// Description of an intent.
    /// </summary>
    public class IntentDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntentDescription"/> class.
        /// </summary>
        /// <param name="name">Intent name.</param>
        public IntentDescription(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets name of the intent.
        /// </summary>
        /// <value>Intent name.</value>
        public string Name { get; }
    }
}
