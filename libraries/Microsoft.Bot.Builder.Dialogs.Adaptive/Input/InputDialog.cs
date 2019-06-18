// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Expressions;
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

    public abstract class InputDialog : DialogCommand
    {
        public bool AlwaysPrompt { get; set; } = false;

        public bool AllowInterruptions { get; set; } = true;

        public Expression Value { get; set; }

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

        public List<Expression> Validations { get; set; } = new List<Expression>();

        public int? MaxTurnCount { get; set; }

        public Expression DefaultValue { get; set; }

        /// <summary>
        /// The property from memory to pass to the calling dialog and to set the return value to.
        /// </summary>
        public override string Property
        {
            get
            {
                return OutputBinding;
            }
            set
            {
                InputBindings["value"] = value;
                OutputBinding = value;
            }
        }

        public static readonly string OPTIONS_PROPERTY = "dialog._input";
        public static readonly string INITIAL_VALUE_PROPERTY = "dialog._input.value";
        public static readonly string TURN_COUNT_PROPERTY = "dialog._input.turnCount";
        public static readonly string INPUT_PROPERTY = "turn._input";

        private const string PersistedOptions = "options";
        private const string PersistedState = "state";

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            var op = OnInitializeOptions(dc, options);
            dc.State.SetValue(OPTIONS_PROPERTY, op);
            dc.State.SetValue(TURN_COUNT_PROPERTY, 0);
            dc.State.SetValue(INPUT_PROPERTY, null);

            var state = this.AlwaysPrompt ? InputState.Missing : await this.RecognizeInput(dc, false);
            if (state == InputState.Valid)
            {
                var input = dc.State.GetValue<object>(INPUT_PROPERTY);
                return await dc.EndDialogAsync(input);
            }
            else
            {
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

            var stepCount = dc.State.GetValue<int>("turn.stepCount", 0);

            if (stepCount > 0)
            {
                return await this.PromptUser(dc, InputState.Missing);
            }

            var turnCount = dc.State.GetValue<int>(TURN_COUNT_PROPERTY, 0) + 1;
            dc.State.SetValue(TURN_COUNT_PROPERTY, turnCount);

            // Perform base recognition
            var state = await this.RecognizeInput(dc, false);

            if (state == InputState.Valid)
            {
                var input = dc.State.GetValue<object>(INPUT_PROPERTY);
                return await dc.EndDialogAsync(input);
            }
            else if (this.MaxTurnCount == null || turnCount < this.MaxTurnCount)
            {
                return await this.PromptUser(dc, state);
            }
            else
            {
                if (this.DefaultValue != null)
                {
                    var (value, error) = this.DefaultValue.TryEvaluate(dc.State);
                    return await dc.EndDialogAsync(value);
                }
            }

            return await dc.EndDialogAsync();
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.PromptUser(dc, InputState.Missing);
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Dialog.EndOfTurn;
        }

        protected abstract Task<InputState> OnRecognizeInput(DialogContext dc, bool consultation);

        protected override async Task<bool> OnPreBubbleEvent(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
        {
            if (e.Name == DialogEvents.ActivityReceived && dc.Context.Activity.Type == ActivityTypes.Message)
            {
                if (this.AllowInterruptions)
                {
                    var state = await this.RecognizeInput(dc, true).ConfigureAwait(false);
                    return state == InputState.Valid;
                }
                else
                {
                    return true;
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

        private async Task<InputState> RecognizeInput(DialogContext dc, bool consultation)
        {
            dynamic input = null;
            if (this.Value != null)
            {
                var (temp, error) = this.Value.TryEvaluate(dc.State);
                input = temp;
            }

            if (input == null)
            {
                var turnCount = dc.State.GetValue<int>(TURN_COUNT_PROPERTY);
                if (turnCount == 0)
                {
                    input = dc.State.GetValue<object>(INITIAL_VALUE_PROPERTY, null);
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
            }

            dc.State.SetValue(INPUT_PROPERTY, input);
            if (input != null)
            {
                var state = await this.OnRecognizeInput(dc, consultation).ConfigureAwait(false);
                if (state == InputState.Valid)
                {
                    foreach (var validation in this.Validations)
                    {
                        var (value, error) = validation.TryEvaluate(dc.State);
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
