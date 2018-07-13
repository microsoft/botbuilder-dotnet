// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    internal static class PromptMessageFactory
    {
        public static MessageActivity CreateActivity(PromptOptions options, bool isRetry)
        {
            if (isRetry)
            {
                if (options.RetryPromptActivity != null)
                {
                    return options.RetryPromptActivity;
                }
                if (options.RetryPromptString != null)
                {
                    return CreateActivity(options.RetryPromptString, options.RetrySpeak);
                }
            }
            // else fall through and use non-retry prompt option

            if (options.PromptActivity != null)
            {
                return options.PromptActivity;
            }
            if (options.PromptString != null)
            {
                return CreateActivity(options.PromptString, options.Speak);
            }

            throw new ArgumentException("Missing required fields on PromptOptions", nameof(options));
        }

        private static MessageActivity CreateActivity(string text, string speak) =>
            new MessageActivity
            { 
                InputHint = InputHints.ExpectingInput,
                Text = !string.IsNullOrWhiteSpace(text) ? text : null,
                Speak = !string.IsNullOrWhiteSpace(speak) ? speak : null,
            };
    }
}
