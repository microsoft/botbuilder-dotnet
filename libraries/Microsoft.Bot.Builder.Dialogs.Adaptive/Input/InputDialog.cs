// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    public abstract class InputDialog : Dialog
    {
#pragma warning disable SA1310 // Field should not contain underscore.
        protected const string TURN_COUNT_PROPERTY = "this.turnCount";
        protected const string VALUE_PROPERTY = "this.value";

        // This property can be set by user's code to indicate that the input should re-process incoming user utterance. 
        // Designed to be a bool property. So user's code can set this to 'true' to signal the input to re-process incoming user utterance.
        protected const string PROCESS_INPUT_PROPERTY = "turn.processInput";
#pragma warning restore SA1310 // Field should not contain underscore.

        /// <summary>
        /// Gets or sets a value indicating whether the input should always prompt the user regardless of there being a value or not.
        /// </summary>
        [JsonProperty("alwaysPrompt")]
        public bool AlwaysPrompt { get; set; } = false;

        /// <summary>
        /// Gets or sets intteruption policy.
        /// </summary>
        [JsonProperty("allowInterruptions")]
        public AllowInterruptions AllowInterruptions { get; set; } = AllowInterruptions.NotRecognized;

        /// <summary>
        /// Gets or sets the value expression which the input will be bound to
        /// </summary>
        [JsonProperty("property")]
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets a value expression which can be used to intialize the input prompt.
        /// </summary>
        /// <remarks>
        /// An example of how to use this would be to use an entity expression such as @age to fill the value for this dialog
        /// that is configured to go into $age dialog property.
        /// </remarks>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the activity to send to the user.
        /// </summary>
        [JsonProperty("prompt")]
        public ITemplate<Activity> Prompt { get; set; }

        /// <summary>
        /// Gets or sets the activity template for retrying prompt.
        /// </summary>
        [JsonProperty("unrecognizedPrompt")]
        public ITemplate<Activity> UnrecognizedPrompt { get; set; }

        /// <summary>
        /// Gets or sets the activity template to send to the user whenever the value provided is invalid.
        /// </summary>
        [JsonProperty("invalidPrompt")]
        public ITemplate<Activity> InvalidPrompt { get; set; }

        /// <summary>
        /// Gets or sets the activity template to send when MaxTurnCount has been reached and the default value is used.
        /// </summary>
        [JsonProperty("defaultValueResponse")]
        public ITemplate<Activity> DefaultValueResponse { get; set; }

        /// <summary>
        /// Gets or sets the expressions to run to validate the input.
        /// </summary>
        [JsonProperty("validations")]
        public List<string> Validations { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets maximum number of times to ask the user for this value before the dilog gives up.
        /// </summary>
        [JsonProperty("maxTurnCount")]
        public int? MaxTurnCount { get; set; }

        /// <summary>
        /// Gets or sets the default value for the input dialog when MaxTurnCount is exceeded.
        /// </summary>
        [JsonProperty("defaultValue")]
        public string DefaultValue { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            var op = OnInitializeOptions(dc, options);
            dc.State.SetValue(ThisPath.OPTIONS, op);
            dc.State.SetValue(TURN_COUNT_PROPERTY, 0);

            if (!String.IsNullOrEmpty(this.Value))
            {
                if (dc.State.TryGetValue(this.Value,  out var value))
                {
                    dc.State.SetValue(VALUE_PROPERTY, value);
                }
            }

            var state = this.AlwaysPrompt ? InputState.Missing : await this.RecognizeInput(dc);
            if (state == InputState.Valid)
            {
                var input = dc.State.GetValue<object>(VALUE_PROPERTY);

                // set property
                dc.State.SetValue(this.Property, input);

                // return as result too
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

            var interrupted = dc.State.GetValue<bool>(TurnPath.INTERRUPTED, () => false);
            var turnCount = dc.State.GetValue<int>(TURN_COUNT_PROPERTY, () => 0);

            // Perform base recognition
            var state = interrupted ? InputState.Missing : await this.RecognizeInput(dc);

            if (state == InputState.Valid)
            {
                var input = dc.State.GetValue<object>(VALUE_PROPERTY);

                // set output property
                if (!string.IsNullOrEmpty(this.Property))
                {
                    dc.State.SetValue(this.Property, input);
                }

                return await dc.EndDialogAsync(input).ConfigureAwait(false);
            }
            else if (this.MaxTurnCount == null || turnCount < this.MaxTurnCount)
            {
                // increase the turnCount as last step
                dc.State.SetValue(TURN_COUNT_PROPERTY, turnCount + 1);
                return await this.PromptUser(dc, state).ConfigureAwait(false);
            }
            else
            {
                if (this.DefaultValue != null)
                {
                    var (value, error) = new ExpressionEngine().Parse(this.DefaultValue).TryEvaluate(dc.State);
                    if (this.DefaultValueResponse != null)
                    {
                        var response = await this.DefaultValueResponse.BindToData(dc.Context, dc.State).ConfigureAwait(false);
                        await dc.Context.SendActivityAsync(response).ConfigureAwait(false);
                    }

                    // set output property
                    dc.State.SetValue(this.Property, value);

                    return await dc.EndDialogAsync(value).ConfigureAwait(false);
                }
            }

            return await dc.EndDialogAsync().ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.PromptUser(dc, InputState.Missing).ConfigureAwait(false);
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

            return await this.Prompt.BindToData(dc.Context, dc.State).ConfigureAwait(false);
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
                input = dc.State.GetValue<object>(this.Property);
            }

            if (input == null)
            {
                var turnCount = dc.State.GetValue<int>(TURN_COUNT_PROPERTY);
                var processInput = dc.State.GetBoolValue(PROCESS_INPUT_PROPERTY, false);

                // Go down this path only if the user has not requested to re-process user input via turn.processInput = true.
                if (turnCount == 0 && !processInput)
                {
                    input = dc.State.GetValue<object>(VALUE_PROPERTY, () => null);
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

            dc.State.SetValue(VALUE_PROPERTY, input);
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
