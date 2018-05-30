// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Basic configuration options supported by all prompts.
    /// </summary>
    public abstract class Prompt<T> : Dialog, IDialogContinue where T : PromptResult
    {
        protected abstract Task OnPrompt(DialogContext dc, PromptOptions options, bool isRetry);

        protected abstract Task<T> OnRecognize(DialogContext dc, PromptOptions options);

        public async Task DialogBegin(DialogContext dc, IDictionary<string, object> dialogArgs)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (dialogArgs == null)
                throw new ArgumentNullException("Prompt options are required for Prompt dialogs");

            var promptOptions = PromptOptions.Create(dialogArgs);

            // Persist options
            var instance = dc.ActiveDialog;
            instance.State = promptOptions;

            // Send initial prompt
            await OnPrompt(dc, promptOptions, false);
        }

        public async Task DialogContinue(DialogContext dc)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));

            // Don't do anything for non-message activities
            if (dc.Context.Activity.Type != ActivityTypes.Message)
            {
                return;
            }

            // Recognize value
            var instance = dc.ActiveDialog;
            var recognized = await OnRecognize(dc, (PromptOptions)instance.State);

            // TODO: resolve the inconsistency of approach between the Node SDK and what we have here
            if (!recognized.Succeeded())
            {
                // TODO: setting this to null is intended to mimicking the behavior of the Node SDK PromptValidator 
                recognized = null;
            }

            if (recognized != null)
            {
                // Return recognized value
                await dc.End(recognized);
            }
            else if (!dc.Context.Responded)
            {
                // Send retry prompt
                await OnPrompt(dc, (PromptOptions)instance.State, true);
            }
        }
    }
}
