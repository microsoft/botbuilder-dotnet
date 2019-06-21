// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Prompts a user to upload attachments, like images.
    /// </summary>
    public class AttachmentPrompt : Prompt<IList<Attachment>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentPrompt"/> class.
        /// </summary>
        /// <param name="dialogId">Unique ID of the dialog within its parent <see cref="DialogSet"/>
        /// or <see cref="ComponentDialog"/>.</param>
        /// <param name="validator">Validator that will be called each time a new activity is received.</param>
        public AttachmentPrompt(string dialogId, PromptValidator<IList<Attachment>> validator = null)
            : base(dialogId, validator)
        {
        }

        protected override async Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, bool isRetry, CancellationToken cancellationToken = default(CancellationToken))
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
                await turnContext.SendActivityAsync(options.RetryPrompt, cancellationToken).ConfigureAwait(false);
            }
            else if (options.Prompt != null)
            {
                await turnContext.SendActivityAsync(options.Prompt, cancellationToken).ConfigureAwait(false);
            }
        }

        protected override Task<PromptRecognizerResult<IList<Attachment>>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
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
