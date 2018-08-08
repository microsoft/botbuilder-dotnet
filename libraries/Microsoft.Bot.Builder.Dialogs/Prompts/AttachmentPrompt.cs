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

        protected override async Task OnPromptAsync(ITurnContext context, IDictionary<string, object> state, PromptOptions options, bool isRetry)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (isRetry && options.RetryPrompt != null)
            {
                await context.SendActivityAsync(options.RetryPrompt).ConfigureAwait(false);
            }
            else if (options.Prompt != null)
            {
                await context.SendActivityAsync(options.Prompt).ConfigureAwait(false);
            }
        }

        protected override Task<PromptRecognizerResult<IList<Attachment>>> OnRecognizeAsync(ITurnContext context, IDictionary<string, object> state, PromptOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var result = new PromptRecognizerResult<IList<Attachment>>();
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var message = context.Activity.AsMessageActivity();
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
