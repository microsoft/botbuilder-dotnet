// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    internal static class PromptMessageFactory
    {
        public static IMessageActivity CreateActivity(PromptOptions options, bool isRetry)
        {
            if (isRetry)
            {
                if (options.RetryPromptActivity != null)
                {
                    return CreateActivity(options.RetryPromptActivity);
                }

                if (options.RetryPromptString != null)
                {
                    return CreateActivity(options.RetryPromptString, options.RetrySpeak);
                }
            }

            // else fall through and use non-retry prompt option
            if (options.PromptActivity != null)
            {
                return CreateActivity(options.PromptActivity);
            }

            if (options.PromptString != null)
            {
                return CreateActivity(options.PromptString, options.Speak);
            }

            throw new ArgumentException("Missing required fields on PromptOptions", nameof(options));
        }

        private static IMessageActivity CreateActivity(string text, string speak)
        {
            var activity = Activity.CreateMessageActivity();
            activity.InputHint = InputHints.ExpectingInput;
            activity.Text = !string.IsNullOrWhiteSpace(text) ? text : null;
            activity.Speak = !string.IsNullOrWhiteSpace(speak) ? speak : null;
            return activity;
        }

        private static IMessageActivity CreateActivity(IActivity activity)
        {
            if (activity.Type != ActivityTypes.Message)
            {
                throw new ArgumentException("Provided Activity must be a Message Activity");
            }

            return activity.AsMessageActivity();
        }
    }
}
