// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs
{

    public abstract class Prompt<TValue> : Prompt<TValue, PromptOptions>
    {
        public Prompt(string dialogId = null, PromptValidator<TValue> validator = null)
            : base(dialogId)
        {
        }
    }

    /// <summary>
    /// Basic configuration options supported by all prompts.
    /// </summary>
    /// <typeparam name="TValue">The type of the <see cref="Prompt{T}"/>.</typeparam>
    public abstract class Prompt<TValue, TPromptOptions> : Dialog
        where TPromptOptions : PromptOptions, new()
    {
        private const string PersistedOptions = "options";
        private const string PersistedState = "state";

        protected PromptValidator<TValue> _validator = null;

        public Prompt(string dialogId = null, PromptValidator<TValue> validator = null)
            : base(dialogId)
        {
            _validator = validator;

            this.InitialPrompt = this.DefineMessageActivityProperty(nameof(InitialPrompt));
            this.RetryPrompt = this.DefineMessageActivityProperty(nameof(RetryPrompt));
            this.NoMatchResponse = this.DefineMessageActivityProperty(nameof(NoMatchResponse));
        }

        /// <summary>
        /// Gets or sets the initial prompt to send the user as <seealso cref="Activity"/>Activity.
        /// </summary>
        /// <value>
        /// The initial prompt to send the user as <seealso cref="Activity"/>Activity.
        /// </value>
        public ITemplate<IMessageActivity> InitialPrompt { get; set; }

        /// <summary>
        /// Gets or sets the retry prompt to send the user as <seealso cref="Activity"/>Activity.
        /// </summary>
        /// <value>
        /// The retry prompt to send the user as <seealso cref="Activity"/>Activity.
        /// </value>
        public ITemplate<IMessageActivity> RetryPrompt { get; set; }

        /// <summary>
        /// Gets or sets the activity to send when the input didn't match at all
        /// </summary>
        public ITemplate<IMessageActivity> NoMatchResponse { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            var promptOptions = new TPromptOptions();

            promptOptions = Object.Assign<TPromptOptions>(promptOptions, options);

            // Ensure prompts have input hint set
            if (promptOptions.Prompt != null && string.IsNullOrEmpty(promptOptions.Prompt.InputHint))
            {
                promptOptions.Prompt.InputHint = InputHints.ExpectingInput;
            }

            if (promptOptions.RetryPrompt != null && string.IsNullOrEmpty(promptOptions.RetryPrompt.InputHint))
            {
                promptOptions.RetryPrompt.InputHint = InputHints.ExpectingInput;
            }

            // Initialize prompt state
            var state = dc.DialogState;
            state[PersistedOptions] = promptOptions;
            state[PersistedState] = new ExpandoObject();

            // Send initial prompt
            await OnBeforePromptAsync(dc, false, cancellationToken).ConfigureAwait(false);
            await OnPromptAsync(dc.Context, (IDictionary<string, object>)state[PersistedState], (TPromptOptions)state[PersistedOptions], false, cancellationToken).ConfigureAwait(false);
            return Dialog.EndOfTurn;
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
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
            var state = (IDictionary<string, object>)dc.DialogState[PersistedState];
            var options = (TPromptOptions)dc.DialogState[PersistedOptions];
            var recognized = await OnRecognizeAsync(dc.Context, state, options, cancellationToken).ConfigureAwait(false);

            // Validate the return value
            var isValid = false;
            if (_validator != null)
            {
                var promptContext = new PromptValidatorContext<TValue>(dc.Context, recognized, state, options);
                isValid = await _validator(promptContext, cancellationToken).ConfigureAwait(false);
            }
            else if (recognized.Succeeded)
            {
                isValid = true;
            }

            // Return recognized value or re-prompt
            if (isValid)
            {
                if (Property != null)
                {
                    dc.State.User[Property] = recognized.Value;
                }

                return await dc.EndDialogAsync(recognized.Value).ConfigureAwait(false);
            }
            else
            {
                if (!dc.Context.Responded)
                {
                    await OnBeforePromptAsync(dc, true, cancellationToken).ConfigureAwait(false);
                    await OnPromptAsync(dc.Context, state, options, true, cancellationToken).ConfigureAwait(false);
                }

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
            await RepromptDialogAsync(dc.Context, dc.ActiveDialog).ConfigureAwait(false);
            return Dialog.EndOfTurn;
        }

        public override async Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = (IDictionary<string, object>)((StateMap)instance.State)[PersistedState];
            var options = (TPromptOptions)((StateMap)instance.State)[PersistedOptions];
            await OnPromptAsync(turnContext, state, options, false).ConfigureAwait(false);
        }

        protected abstract Task OnPromptAsync(ITurnContext turnContext, IDictionary<string, object> state, TPromptOptions options, bool isRetry, CancellationToken cancellationToken = default(CancellationToken));

        protected virtual async Task OnBeforePromptAsync(DialogContext dc, bool isRetry, CancellationToken cancellationToken = default(CancellationToken))
        {
        }

        protected abstract Task<PromptRecognizerResult<TValue>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, TPromptOptions options, CancellationToken cancellationToken = default(CancellationToken));

        protected IMessageActivity AppendChoices(IMessageActivity prompt, string channelId, IList<Choice> choices, ListStyle style, ChoiceFactoryOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get base prompt text (if any)
            var text = prompt != null && !string.IsNullOrEmpty(prompt.Text) ? prompt.Text : string.Empty;

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
                // clone the prompt the set in the options (note ActivityEx has Properties so this is the safest mechanism)
                prompt = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(prompt));

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
