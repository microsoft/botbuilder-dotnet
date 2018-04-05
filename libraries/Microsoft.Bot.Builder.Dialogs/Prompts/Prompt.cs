// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Prompts;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Basic configuration options supported by all prompts.
    /// </summary>
    public abstract class Prompt<T> : Control where T : PromptResult
    {
        public Prompt()
        {
        }

        public override bool HasDialogContinue => true;

        public override bool HasDialogResume => false;

        protected abstract Task OnPrompt(DialogContext dc, PromptOptions options, bool isRetry);

        protected abstract Task<T> OnRecognize(DialogContext dc, PromptOptions options);

        public override async Task DialogBegin(DialogContext dc, object dialogArgs)
        {
            var promptOptions = (PromptOptions)dialogArgs;

            // Persist options
            var instance = dc.Instance;
            instance.State = promptOptions;

            // Send initial prompt
            await OnPrompt(dc, promptOptions, false);
        }

        public override async Task DialogContinue(DialogContext dc)
        {
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
