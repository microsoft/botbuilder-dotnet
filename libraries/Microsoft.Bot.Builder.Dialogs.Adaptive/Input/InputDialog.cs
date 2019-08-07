// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using static Microsoft.Bot.Builder.Dialogs.DialogContext;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    public class InputDialogOptions
    {
    }

    public enum InputState
    {
        Missing,
        Unrecognized,
        Invalid,
        Valid
    }

    public enum AllowInterruptions
    {
        /**
         * always consult parent dialogs before taking the input 
         */
        Always,

        /**
         * never consult parent dialogs 
         */
        Never,

        /**
         * recognize the input first, only consult parent dilaogs when notRecognized
         */
        NotRecognized
    }

    public abstract class InputDialog : Dialog
    {
        private Expression value;
        private Expression defaultValue;

        public bool AlwaysPrompt { get; set; } = false;

        public AllowInterruptions AllowInterruptions { get; set; } = AllowInterruptions.NotRecognized;

        /// <summary>
        /// Initial value for the prompt
        /// </summary>
        [JsonProperty("value")]
        public string Value
        {
            get { return value?.ToString(); }
            set { this.value = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Activity to send to the user
        /// </summary>
        public ITemplate<Activity> Prompt { get; set; }

        /// <summary>
        /// Activity template for retrying prompt
        /// </summary>
        public ITemplate<Activity> UnrecognizedPrompt { get; set; }

        /// <summary>
        /// Activity template to send to the user whenever the value provided is invalid
        /// </summary>
        public ITemplate<Activity> InvalidPrompt { get; set; }

        public List<string> Validations { get; set; } = new List<string>();

        /// <summary>
        /// Maximum number of times to ask the user for this value before the dilog gives up.
        /// </summary>
        public int? MaxTurnCount { get; set; }

        /// <summary>
        /// Default value for the input dialog
        /// </summary>
        public string DefaultValue
        {
            get { return defaultValue?.ToString(); }
            set { lock (this) defaultValue = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// The property from memory to pass to the calling dialog and to set the return value to.
        /// </summary>
        public string Property
        {
            get
            {
                return OutputBinding;
            }
            set
            {
                InputBindings[DialogContextState.DIALOG_VALUE] = value;
                OutputBinding = value;
            }
        }

        public const string TURN_COUNT_PROPERTY = "dialog.turnCount";
        public const string INPUT_PROPERTY = "turn.value";

        // This property can be set by user's code to indicate that the input should re-process incoming user utterance. 
        // Designed to be a bool property. So user's code can set this to 'true' to signal the input to re-process incoming user utterance.

        public const string PROCESS_INPUT_PROPERTY = "turn.processInput";
        private const string PersistedOptions = "options";
        private const string PersistedState = "state";

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            var op = OnInitializeOptions(dc, options);
            dc.State.SetValue(DialogContextState.DIALOG_OPTIONS, op);
            dc.State.SetValue(TURN_COUNT_PROPERTY, 0);
            dc.State.SetValue(INPUT_PROPERTY, null);

            var state = this.AlwaysPrompt ? InputState.Missing : await this.RecognizeInput(dc);
            if (state == InputState.Valid)
            {
                var input = dc.State.GetValue<object>(INPUT_PROPERTY);
                return await dc.EndDialogAsync(input);
            }
            else
            {
                // turnCount should increase here, because you want when nextTurn comes in
                // We will set the turn count to 1 so the input will not pick from "dialog.value"
                // and instead go with "turn.activity.text"
                dc.State.SetValue(TURN_COUNT_PROPERTY, 1);
                return await this.PromptUser(dc, state);
            }
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var activity = dc.Context.Activity;
            if (activity.Type != ActivityTypes.Message)
            {
                return Dialog.EndOfTurn;
            }

            var stepCount = dc.State.GetValue<int>(DialogContextState.TURN_STEPCOUNT, 0);

            if (stepCount > 0)
            {
                return await this.PromptUser(dc, InputState.Missing);
            }

            var turnCount = dc.State.GetValue<int>(TURN_COUNT_PROPERTY, 0);

            // Perform base recognition
            var state = await this.RecognizeInput(dc);

            if (state == InputState.Valid)
            {
                var input = dc.State.GetValue<object>(INPUT_PROPERTY);
                return await dc.EndDialogAsync(input);
            }
            else if (this.MaxTurnCount == null || turnCount < this.MaxTurnCount)
            {
                // increase the turnCount as last step
                dc.State.SetValue(TURN_COUNT_PROPERTY, turnCount + 1);
                return await this.PromptUser(dc, state);
            }
            else
            {
                if (this.defaultValue != null)
                {
                    var (value, error) = this.defaultValue.TryEvaluate(dc.State);
                    return await dc.EndDialogAsync(value);
                }
            }

            return await dc.EndDialogAsync();
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.PromptUser(dc, InputState.Missing);
        }

        protected abstract Task<InputState> OnRecognizeInput(DialogContext dc);

        protected override async Task<bool> OnPreBubbleEvent(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
        {
            if (e.Name == DialogEvents.ActivityReceived && dc.Context.Activity.Type == ActivityTypes.Message)
            {
                switch (this.AllowInterruptions)
                {
                    case AllowInterruptions.Always:
                        return false;

                    case AllowInterruptions.Never:
                        return true;

                    case AllowInterruptions.NotRecognized:
                        var state = await this.RecognizeInput(dc).ConfigureAwait(false);

                        // RecognizedInput can come back with different InputState enum values. 
                        // We need to have predictible behavior for users here so when NotRecognized is set
                        //    we do not bubble up when InputState is either Valid or Invalid. 
                        //    not consulting on Invalid is critical so the bot can continue to re-prompt the user 
                        //    or in the ideal case, render 'InvalidPrompt' if user has specified one. 
                        // RecognizeInput => 
                        //      InputState.Invalid      -> Do not bubble up -> return true
                        //      InputState.Valid        -> Do not bubble up -> return true
                        //      InputState.Missing      -> bubble up        -> return false
                        //      InputState.Unrecognized -> bubble up        -> return false

                        return state == InputState.Valid || state == InputState.Invalid;
                }
            }

            return false;
        }

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

                case ListStyle.HeroCard:
                    msg = ChoiceFactory.HeroCard(choices, text);
                    break;

                case ListStyle.None:
                    msg = Activity.CreateMessageActivity();
                    msg.Text = text;
                    break;

                default:
                    msg = ChoiceFactory.ForChannel(channelId, choices, text, null, options);
                    break;
            }

            // Update prompt with text, actions and attachments
            if (prompt != null)
            {
                // clone the prompt the set in the options (note ActivityEx has Properties so this is the safest mechanism)
                prompt = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(prompt));

                prompt.Text = msg.Text;

                if (msg.SuggestedActions != null && msg.SuggestedActions.Actions != null && msg.SuggestedActions.Actions.Count > 0)
                {
                    prompt.SuggestedActions = msg.SuggestedActions;
                }

                if (msg.Attachments != null && msg.Attachments.Any())
                {
                    prompt.Attachments = msg.Attachments;
                }

                return prompt;
            }
            else
            {
                msg.InputHint = InputHints.ExpectingInput;
                return msg;
            }
        }

        protected virtual object OnInitializeOptions(DialogContext dc, object options)
        {
            return options;
        }

        protected virtual async Task<IActivity> OnRenderPrompt(DialogContext dc, InputState state)
        {
            switch (state)
            {
                case InputState.Unrecognized:
                    if (this.UnrecognizedPrompt != null)
                    {
                        return await this.UnrecognizedPrompt.BindToData(dc.Context, dc.State).ConfigureAwait(false);
                    }
                    else if (this.InvalidPrompt != null)
                    {
                        return await this.InvalidPrompt.BindToData(dc.Context, dc.State).ConfigureAwait(false);
                    }
                    break;

                case InputState.Invalid:
                    if (this.InvalidPrompt != null)
                    {
                        return await this.InvalidPrompt.BindToData(dc.Context, dc.State).ConfigureAwait(false);
                    }
                    else if (this.UnrecognizedPrompt != null)
                    {
                        return await this.UnrecognizedPrompt.BindToData(dc.Context, dc.State).ConfigureAwait(false);
                    }
                    break;

            }

            return await this.Prompt.BindToData(dc.Context, dc.State);
        }

        private async Task<InputState> RecognizeInput(DialogContext dc)
        {
            dynamic input = null;

            // If AlwaysPrompt is set to true, the Property value will be cleared.
            if (!string.IsNullOrEmpty(this.Property) && this.AlwaysPrompt)
            {
                dc.State.SetValue(this.Property, null);
            }

            // If AlwaysPrompt is set to false, try to get the Property value first.
            if (!string.IsNullOrEmpty(this.Property) && !this.AlwaysPrompt)
            {
                input = dc.State.GetValue(this.Property, null);
            }

            if (this.Value != null)
            {
                input = dc.State.GetValue(this.value, null);
            }

            if (input == null)
            {
                var turnCount = dc.State.GetValue<int>(TURN_COUNT_PROPERTY);
                var processInput = dc.State.GetValue<bool>(PROCESS_INPUT_PROPERTY, false);

                // Go down this path only if the user has not requested to re-process user input via turn.processInput = true.
                if (turnCount == 0 && !processInput)
                {
                    input = dc.State.GetValue<object>(DialogContextState.DIALOG_VALUE, null);
                }
                else
                {
                    if (this.GetType().Name == nameof(AttachmentInput))
                    {
                        input = dc.Context.Activity.Attachments;
                    }
                    else
                    {
                        input = dc.Context.Activity.Text;
                    }
                }

                // reset turn.processInput so subsequent actions are not impacted. 
                dc.State.SetValue(PROCESS_INPUT_PROPERTY, false);
            }

            dc.State.SetValue(INPUT_PROPERTY, input);
            if (input != null)
            {
                var state = await this.OnRecognizeInput(dc).ConfigureAwait(false);
                if (state == InputState.Valid)
                {
                    foreach (var validation in this.Validations)
                    {
                        var exp = new ExpressionEngine().Parse(validation);
                        var (value, error) = exp.TryEvaluate(dc.State);
                        if (value == null || (value is bool && (bool)value == false))
                        {
                            return InputState.Invalid;
                        }
                    }
                    return InputState.Valid;
                }
                else
                {
                    return state;
                }
            }
            else
            {
                return InputState.Missing;
            }
        }

        private async Task<DialogTurnResult> PromptUser(DialogContext dc, InputState state)
        {
            var prompt = await this.OnRenderPrompt(dc, state).ConfigureAwait(false);
            await dc.Context.SendActivityAsync(prompt).ConfigureAwait(false);
            return Dialog.EndOfTurn;
        }
    }
}
