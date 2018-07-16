// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Basic configuration options supported by all prompts.
    /// </summary>
    public abstract class Prompt<T> : Dialog, IDialogContinue
        where T : PromptResult
    {
        protected abstract Task OnPromptAsync(DialogContext dc, PromptOptions options, bool isRetry);

        protected abstract Task<T> OnRecognizeAsync(DialogContext dc, PromptOptions options);

        public async Task DialogBeginAsync(DialogContext dc, IDictionary<string, object> dialogArgs)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (dialogArgs == null)
            {
                throw new ArgumentNullException(nameof(dialogArgs), "Prompt options are required for Prompt dialogs");
            }

            var promptOptions = PromptOptions.Create(dialogArgs);

            // Persist options
            var instance = dc.ActiveDialog;
            instance.State = promptOptions;

            // Send initial prompt
            await OnPromptAsync(dc, promptOptions, false);
        }

        public async Task DialogContinueAsync(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Don't do anything for non-message activities
            if (dc.Context.Activity.Type != ActivityTypes.Message)
            {
                return;
            }

            // Recognize value
            var instance = dc.ActiveDialog;
            var recognized = await OnRecognizeAsync(dc, (PromptOptions)instance.State);

            // TODO: resolve the inconsistency of approach between the Node SDK and what we have here
            if (!recognized.Succeeded())
            {
                // TODO: setting this to null is intended to mimicking the behavior of the Node SDK PromptValidator
                recognized = null;
            }

            if (recognized != null)
            {
                // Return recognized value
                await dc.EndAsync(recognized);
            }
            else if (!dc.Context.Responded)
            {
                // Send retry prompt
                await OnPromptAsync(dc, (PromptOptions)instance.State, true);
            }
        }
    }
}
