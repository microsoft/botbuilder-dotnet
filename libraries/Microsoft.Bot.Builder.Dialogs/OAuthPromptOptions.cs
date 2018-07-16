// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Prompt Options for an OAuth Prompt.
    /// </summary>
    public class OAuthPromptOptions : PromptOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthPromptOptions"/> class.
        /// Default Constructor for serialization/deserialization.
        /// </summary>
        public OAuthPromptOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthPromptOptions"/> class from a base prompt options object.
        /// If null is passed as default, then fallback to just default options for the base class.
        /// </summary>
        /// <param name="defaultPromptOptions">The defualt options for the prompt.</param>
        public OAuthPromptOptions(PromptOptions defaultPromptOptions)
            : base()
        {
            if (defaultPromptOptions != null)
            {
                PromptString = PromptString ?? defaultPromptOptions.PromptString;
                PromptActivity = PromptActivity ?? defaultPromptOptions.PromptActivity;
                Speak = Speak ?? defaultPromptOptions.Speak;
                RetryPromptString = RetryPromptString ?? defaultPromptOptions.RetryPromptString;
                RetryPromptActivity = RetryPromptActivity ?? defaultPromptOptions.RetryPromptActivity;
                RetrySpeak = RetrySpeak ?? defaultPromptOptions.RetrySpeak;
            }
        }

        /// <summary>
        /// Gets or sets the expire timestamp for the oauth prompt.
        /// </summary>
        /// <value>
        /// The expire timestamp for the oauth prompt.
        /// </value>
        public DateTime Expires { get; set; }
    }
}
