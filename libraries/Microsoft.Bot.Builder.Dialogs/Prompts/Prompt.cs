// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Basic configuration options supported by all prompts.
    /// </summary>
    public abstract class Prompt<T> : Control, IDialogContinue where T : PromptResult
    {
        public Prompt()
        {
        }

        protected abstract Task OnPrompt(DialogContext dc, PromptOptions options, bool isRetry);

        protected abstract Task<T> OnRecognize(DialogContext dc, PromptOptions options);

        public async Task DialogBegin(DialogContext dc, object dialogArgs)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));
            if (dialogArgs == null)
                throw new ArgumentNullException(nameof(dialogArgs));

            var promptOptions = (PromptOptions)dialogArgs;

            // Persist options
            var instance = dc.Instance;
            instance.State = promptOptions;

            // Send initial prompt
            await OnPrompt(dc, promptOptions, false);
        }

        public async Task DialogContinue(DialogContext dc)
        {
            if (dc == null)
                throw new ArgumentNullException(nameof(dc));

            if (dc.Context.Activity.Type != ActivityTypes.Message)
            {
                return;
            }

            // Recognize value
            var instance = dc.Instance;
            var recognized = await OnRecognize(dc, (PromptOptions)instance.State);

            // TODO: resolve the inconsistency of approach between the Node SDK and what we have here
            if (!recognized.Succeeded())
            {
                // TODO: setting this to null is intended to mimicking the behavior of the Node SDK PromptValidator 
                recognized = null;
            }

            if (recognized != null)
            {
                await dc.End(recognized);
            }
            else if (!dc.Context.Responded)
            {
                await OnPrompt(dc, (PromptOptions)instance.State, true);
            }
        }
    }
}
