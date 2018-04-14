// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class PromptOptions : IDialogOptions
    {
        public PromptOptions()
        {
        }

        public object ApplyDefaults(object defaults)
        {
            var defaultPromptOptions = (PromptOptions)defaults;
            return new PromptOptions
            {
                PromptString = PromptString ?? defaultPromptOptions.PromptString,
                PromptActivity = PromptActivity ?? defaultPromptOptions.PromptActivity,
                Speak = Speak ?? defaultPromptOptions.Speak,
                RetryPromptString = RetryPromptString ?? defaultPromptOptions.RetryPromptString,
                RetryPromptActivity = RetryPromptActivity ?? defaultPromptOptions.RetryPromptActivity,
                RetrySpeak = RetrySpeak ?? defaultPromptOptions.RetrySpeak
            };
        }

        /// <summary>
        /// (Optional) Initial prompt to send the user. As string.
        /// </summary>
        public string PromptString { get; set; }

        /// <summary>
        /// (Optional) Initial prompt to send the user. As Activity.
        /// </summary>
        public Activity PromptActivity { get; set; }

        /// <summary>
        /// (Optional) Initial SSML to send the user.
        /// </summary>
        public string Speak { get; set; }

        /// <summary>
        /// (Optional) Retry prompt to send the user. As String.
        /// </summary>
        public string RetryPromptString { get; set; }

        /// <summary>
        /// (Optional) Retry prompt to send the user. As Activity.
        /// </summary>
        public Activity RetryPromptActivity { get; set; }

        /// <summary>
        /// (Optional) Retry SSML to send the user.
        /// </summary>
        public string RetrySpeak { get; set; }
    }
}
