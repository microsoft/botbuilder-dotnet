// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class AttachmentPrompt : Prompt<IList<Attachment>>
    {
        public AttachmentPrompt(string dialogId, PromptValidator<IList<Attachment>> validator = null)
            : base(dialogId, validator)
        {
        }

        protected override async Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, bool isRetry)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (isRetry && options.RetryPrompt != null)
            {
                await turnContext.SendActivityAsync(options.RetryPrompt).ConfigureAwait(false);
            }
            else if (options.Prompt != null)
            {
                await turnContext.SendActivityAsync(options.Prompt).ConfigureAwait(false);
            }
        }

        protected override Task<PromptRecognizerResult<IList<Attachment>>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var result = new PromptRecognizerResult<IList<Attachment>>();
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var message = turnContext.Activity.AsMessageActivity();
                if (message.Attachments != null && message.Attachments.Count > 0)
                {
                    result.Succeeded = true;
                    result.Value = message.Attachments;
                }
            }

            return Task.FromResult(result);
        }
    }
}
