// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// OAuth Related Prompt Settings Which allows a Timeout parameter to be set.
    /// The timeout determines the validity of the token.
    /// </summary>
    public class OAuthPromptSettingsWithTimeout : OAuthPromptSettings
    {
        public int? Timeout { get; set; }
    }

    /// <summary>
    /// Prompt Options for an OAuth Prompt.
    /// </summary>
    public class OAuthPromptOptions : PromptOptions
    {
        /// <summary>
        /// Default Constructor for serialization/deserialization
        /// </summary>
        public OAuthPromptOptions() : base()
        {
        }

        /// <summary>
        /// Constuct a OAuth prompt options from a base prompt options object.
        /// If null is passed as default, then fallback to just default options for the base class.
        /// </summary>
        /// <param name="defaultPromptOptions"></param>
        public OAuthPromptOptions(PromptOptions defaultPromptOptions) : base()
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
        /// The expiry timestamp for the oauth prompt
        /// </summary>
        public DateTime Expires { get; set; }
    }
}
