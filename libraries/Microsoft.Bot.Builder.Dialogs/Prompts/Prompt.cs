// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Basic configuration options supported by all prompts.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Prompt{T}"/>.</typeparam>
    public abstract class Prompt<T> : Dialog
    {
        private const string PersistedOptions = "options";
        private const string PersistedState = "state";

        private readonly PromptValidator<T> _validator;

        public Prompt(string dialogId, PromptValidator<T> validator = null)
            : base(dialogId)
        {
            _validator = validator;
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
            await OnPromptAsync(dc.Context, (IDictionary<string, object>)state[PersistedState], (PromptOptions)state[PersistedOptions], false).ConfigureAwait(false);
            return Dialog.EndOfTurn;
        }

        public override async Task<DialogTurnResult> DialogContinueAsync(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Don't do anything for non-message activities
            if (dc.Context.Activity.Type != ActivityTypes.Message)
            {
                return Dialog.EndOfTurn;
            }

            // Perform base recognition
            var instance = dc.ActiveDialog;
            var state = (IDictionary<string, object>)instance.State[PersistedState];
            var options = (PromptOptions)instance.State[PersistedOptions];
            var recognized = await OnRecognizeAsync(dc.Context, state, options).ConfigureAwait(false);

            // Validate the return value
            var end = false;
            object endResult = null;
            if (_validator != null)
            {
                var prompt = new PromptValidatorContext<T>(dc, state, options, recognized);
                await _validator(dc.Context, prompt).ConfigureAwait(false);
                end = prompt.HasEnded;
                endResult = prompt.EndResult;
            }
            else if (recognized.Succeeded)
            {
                end = true;
                endResult = recognized.Value;
            }

            // Return recognized value or re-prompt
            if (end)
            {
                return await dc.EndAsync(endResult).ConfigureAwait(false);
            }
            else
            {
                if (!dc.Context.Responded)
                {
                    await OnPromptAsync(dc.Context, state, options, true).ConfigureAwait(false);
                }

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
            await OnPromptAsync(context, state, options, false).ConfigureAwait(false);
        }

        protected abstract Task OnPromptAsync(ITurnContext context, IDictionary<string, object> state, PromptOptions options, bool isRetry);

        protected abstract Task<PromptRecognizerResult<T>> OnRecognizeAsync(ITurnContext context, IDictionary<string, object> state, PromptOptions options);

        protected IMessageActivity AppendChoices(IMessageActivity prompt, string channelId, IList<Choice> choices, ListStyle style, ChoiceFactoryOptions options = null)
        {
            // Get base prompt text (if any)
            string text = prompt != null && !string.IsNullOrEmpty(prompt.Text) ? prompt.Text : string.Empty;

            // Create temporary msg
            IMessageActivity msg;
            switch (style)
            {
                case ListStyle.Inline:
                    msg = ChoiceFactory.Inline(choices, text, null, options);
                    break;

                case ListStyle.List:
                    msg = ChoiceFactory.List(choices, text, null, options);
                    break;

                case ListStyle.SuggestedAction:
                    msg = ChoiceFactory.SuggestedAction(choices, text);
                    break;

                case ListStyle.None:
                    msg = Activity.CreateMessageActivity();
                    msg.Text = text;
                    break;

                default:
                    msg = ChoiceFactory.ForChannel(channelId, choices, text, null, options);
                    break;
            }

            // Update prompt with text and actions
            if (prompt != null)
            {
                prompt.Text = msg.Text;
                if (msg.SuggestedActions != null && msg.SuggestedActions.Actions != null && msg.SuggestedActions.Actions.Count > 0)
                {
                    prompt.SuggestedActions = msg.SuggestedActions;
                }

                return prompt;
            }
            else
            {
                msg.InputHint = InputHints.ExpectingInput;
                return msg;
            }
        }
    }
}
