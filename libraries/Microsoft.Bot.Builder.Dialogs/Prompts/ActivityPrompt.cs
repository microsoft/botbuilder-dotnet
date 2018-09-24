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

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default(CancellationToken))
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
            await OnPromptAsync(dc.Context, (IDictionary<string, object>)state[PersistedState], (PromptOptions)state[PersistedOptions], cancellationToken).ConfigureAwait(false);
            return Dialog.EndOfTurn;
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Perform base recognition
            var instance = dc.ActiveDialog;
            var state = (IDictionary<string, object>)instance.State[PersistedState];
            var options = (PromptOptions)instance.State[PersistedOptions];
            var recognized = await OnRecognizeAsync(dc.Context, state, options, cancellationToken).ConfigureAwait(false);

            // Validate the return value
            var promptContext = new PromptValidatorContext<Activity>(dc.Context, recognized, state, options);
            var isValid = await _validator(promptContext, cancellationToken).ConfigureAwait(false);

            // Return recognized value or re-prompt
            if (isValid)
            {
                return await dc.EndDialogAsync(recognized.Value, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return Dialog.EndOfTurn;
            }
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Prompts are typically leaf nodes on the stack but the dev is free to push other dialogs
            // on top of the stack which will result in the prompt receiving an unexpected call to
            // dialogResume() when the pushed on dialog ends.
            // To avoid the prompt prematurely ending we need to implement this method and
            // simply re-prompt the user.
            await RepromptDialogAsync(dc.Context, dc.ActiveDialog, cancellationToken).ConfigureAwait(false);
            return Dialog.EndOfTurn;
        }

        public override async Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = (IDictionary<string, object>)instance.State[PersistedState];
            var options = (PromptOptions)instance.State[PersistedOptions];
            await OnPromptAsync(turnContext, state, options, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Prompt != null)
            {
                await turnContext.SendActivityAsync(options.Prompt, cancellationToken).ConfigureAwait(false);
            }
        }

        protected virtual Task<PromptRecognizerResult<Activity>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(new PromptRecognizerResult<Activity>
            {
                Succeeded = true,
                Value = turnContext.Activity,
            });
        }
    }
}
