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
    public abstract class ActivityPrompt : Dialog
    {
        private const string PersistedOptions = "options";
        private const string PersistedState = "state";

        private readonly PromptValidator<Activity> _validator;

        public ActivityPrompt(string dialogId, PromptValidator<Activity> validator)
            : base(dialogId)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public override async Task<DialogTurnResult> DialogBeginAsync(DialogContext dc, DialogOptions options)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (!(options is PromptOptions))
            {
                throw new ArgumentOutOfRangeException(nameof(options), "Prompt options are required for Prompt dialogs");
            }

            // Ensure prompts have input hint set
            var opt = (PromptOptions)options;
            if (opt.Prompt != null && string.IsNullOrEmpty(opt.Prompt.InputHint))
            {
                opt.Prompt.InputHint = InputHints.ExpectingInput;
            }

            if (opt.RetryPrompt != null && string.IsNullOrEmpty(opt.RetryPrompt.InputHint))
            {
                opt.RetryPrompt.InputHint = InputHints.ExpectingInput;
            }

            // Initialize prompt state
            var state = dc.ActiveDialog.State;
            state[PersistedOptions] = opt;
            state[PersistedState] = new Dictionary<string, object>();

            // Send initial prompt
            await OnPromptAsync(dc.Context, (IDictionary<string, object>)state[PersistedState], (PromptOptions)state[PersistedOptions]).ConfigureAwait(false);
            return Dialog.EndOfTurn;
        }

        public override async Task<DialogTurnResult> DialogContinueAsync(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Perform base recognition
            var instance = dc.ActiveDialog;
            var state = (IDictionary<string, object>)instance.State[PersistedState];
            var options = (PromptOptions)instance.State[PersistedOptions];
            var recognized = await OnRecognizeAsync(dc.Context, state, options).ConfigureAwait(false);

            // Validate the return value
            var prompt = new PromptValidatorContext<Activity>(dc, state, options, recognized);
            await _validator(dc.Context, prompt).ConfigureAwait(false);

            // Return recognized value or re-prompt
            if (prompt.HasEnded)
            {
                return await dc.EndAsync(prompt.EndResult).ConfigureAwait(false);
            }
            else
            {
                return Dialog.EndOfTurn;
            }
        }

        public override async Task<DialogTurnResult> DialogResumeAsync(DialogContext dc, DialogReason reason, object result = null)
        {
            // Prompts are typically leaf nodes on the stack but the dev is free to push other dialogs
            // on top of the stack which will result in the prompt receiving an unexpected call to
            // dialogResume() when the pushed on dialog ends.
            // To avoid the prompt prematurely ending we need to implement this method and
            // simply re-prompt the user.
            await DialogRepromptAsync(dc.Context, dc.ActiveDialog).ConfigureAwait(false);
            return Dialog.EndOfTurn;
        }

        public override async Task DialogRepromptAsync(ITurnContext context, DialogInstance instance)
        {
            var state = (IDictionary<string, object>)instance.State[PersistedState];
            var options = (PromptOptions)instance.State[PersistedOptions];
            await OnPromptAsync(context, state, options).ConfigureAwait(false);
        }

        protected virtual async Task OnPromptAsync(ITurnContext context, IDictionary<string, object> state, PromptOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Prompt != null)
            {
                await context.SendActivityAsync(options.Prompt).ConfigureAwait(false);
            }
        }

        protected virtual Task<PromptRecognizerResult<Activity>> OnRecognizeAsync(ITurnContext context, IDictionary<string, object> state, PromptOptions options)
        {
            return Task.FromResult(new PromptRecognizerResult<Activity>
            {
                Succeeded = true,
                Value = context.Activity,
            });
        }
    }
}
