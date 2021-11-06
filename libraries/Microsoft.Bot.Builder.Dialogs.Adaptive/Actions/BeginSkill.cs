﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Begin a Skill.
    /// </summary>
    public class BeginSkill : SkillDialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.BeginSkill";

        // Used to cache DialogOptions for multi-turn calls across servers
        private readonly string _dialogOptionsStateKey = $"{typeof(BeginSkill).FullName}.DialogOptionsData";

        [JsonConstructor]
        public BeginSkill([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(new SkillDialogOptions())
        {
            DialogOptions.Skill = new BotFrameworkSkill();
            RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to have the new dialog should process the activity.
        /// </summary>
        /// <value>
        /// The default for this will be true, which means the new dialog will use the <see cref="Activity"/> to start the conversation with the skill.
        /// You can set this to false to dispatch the activity in the current turn context to the skill.
        /// </value>
        [DefaultValue(true)]
        [JsonProperty("activityProcessed")]
        public BoolExpression ActivityProcessed { get; set; } = true;

        /// <summary>
        /// Gets or sets the property path to store the dialog result in.
        /// </summary>
        /// <value>
        /// The property path to store the dialog result in.
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        /// <summary>
        /// Gets or sets the Microsoft App ID that will be calling the skill.
        /// </summary>
        /// <value>
        /// The Microsoft App ID that will be calling the skill.
        /// </value>
        [JsonProperty("botId")]
        public StringExpression BotId { get; set; } = "=settings.MicrosoftAppId";

        /// <summary>
        /// Gets or sets the callback Url for the skill host.
        /// </summary>
        /// <value>
        /// The callback Url for the skill host.
        /// </value>
        [JsonProperty("skillHostEndpoint")]
        public StringExpression SkillHostEndpoint { get; set; } = "=settings.SkillHostEndpoint";

        /// <summary>
        /// Gets or sets the Microsoft App ID for the skill.
        /// </summary>
        /// <value>
        /// The Microsoft App ID for the skill.
        /// </value>
        [JsonProperty("skillAppId")]
        public StringExpression SkillAppId { get; set; }

        /// <summary>
        /// Gets or sets the /api/messages endpoint for the skill.
        /// </summary>
        /// <value>
        /// The /api/messages endpoint for the skill.
        /// </value>
        [JsonProperty("skillEndpoint")]
        public StringExpression SkillEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the OAuth Connection Name, that would be used to perform Single SignOn with a skill.
        /// </summary>
        /// <value>
        /// The OAuth Connection Name for the Parent Bot.
        /// </value>
        [JsonProperty("connectionName")]
        public StringExpression ConnectionName { get; set; } = "=settings.connectionName";

        /// <summary>
        /// Gets or sets template for the activity.
        /// </summary>
        /// <value>
        /// Template for the activity.
        /// </value>
        [JsonProperty("activity")]
        public ITemplate<Activity> Activity { get; set; }

        /// <summary>
        /// Gets or sets intteruption policy. 
        /// </summary>
        /// <value>
        /// Bool or expression which evalutes to bool.
        /// </value>
        [JsonProperty("allowInterruptions")]
        public BoolExpression AllowInterruptions { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // Update the dialog options with the runtime settings.
            DialogOptions.BotId = BotId.GetValue(dc.State);
            DialogOptions.SkillHostEndpoint = new Uri(SkillHostEndpoint.GetValue(dc.State));
            DialogOptions.ConversationIdFactory = dc.Context.TurnState.Get<SkillConversationIdFactoryBase>() ?? throw new NullReferenceException("Unable to locate SkillConversationIdFactoryBase in HostContext");
            DialogOptions.SkillClient = dc.Context.TurnState.Get<BotFrameworkClient>() ?? throw new NullReferenceException("Unable to locate BotFrameworkClient in HostContext");
            DialogOptions.ConversationState = dc.Context.TurnState.Get<ConversationState>() ?? throw new NullReferenceException($"Unable to get an instance of {nameof(ConversationState)} from TurnState.");
            DialogOptions.ConnectionName = ConnectionName.GetValue(dc.State);

            // Set the skill to call
            DialogOptions.Skill.Id = DialogOptions.Skill.AppId = SkillAppId.GetValue(dc.State);
            DialogOptions.Skill.SkillEndpoint = new Uri(SkillEndpoint.GetValue(dc.State));

            // Store the initialized DialogOptions in state so we can restore these values when the dialog is resumed.
            dc.ActiveDialog.State[_dialogOptionsStateKey] = DialogOptions;

            // Get the activity to send to the skill.
            Activity activity;
            if (ActivityProcessed.GetValue(dc.State))
            {
                // The parent consumed the activity in context, use the Activity property to start the skill.
                activity = await Activity.BindAsync(dc, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Send the turn context activity to the skill (pass through). 
                activity = dc.Context.Activity;
            }

            // Call the base to invoke the skill
            return await base.BeginDialogAsync(dc, new BeginSkillDialogOptions { Activity = activity }, cancellationToken).ConfigureAwait(false);
        }

        public override Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            LoadDialogOptions(dc.Context, dc.ActiveDialog);
            if (dc.Context.Activity.Type == ActivityTypes.EndOfConversation)
            {
                // Capture the result of the dialog if the property is set
                if (ResultProperty != null)
                {
                    dc.State.SetValue(ResultProperty.GetValue(dc.State), dc.Context.Activity.Value);
                }
            }

            return base.ContinueDialogAsync(dc, cancellationToken);
        }

        public override Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default)
        {
            LoadDialogOptions(turnContext, instance);
            return base.RepromptDialogAsync(turnContext, instance, cancellationToken);
        }

        public override Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            LoadDialogOptions(dc.Context, dc.ActiveDialog);
            return base.ResumeDialogAsync(dc, reason, result, cancellationToken);
        }

        public override Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default)
        {
            LoadDialogOptions(turnContext, instance);
            return base.EndDialogAsync(turnContext, instance, reason, cancellationToken);
        }

        protected override string OnComputeId()
        {
            var appId = SkillAppId?.ToString() ?? string.Empty;
            string activity;

            if (Activity is ActivityTemplate at)
            {
                activity = StringUtils.Ellipsis(at.Template.Trim(), 30);
            }
            else
            {
                activity = StringUtils.Ellipsis(Activity?.ToString().Trim(), 30);
            }

            return $"{GetType().Name}['{appId}','{activity}']";
        }

        protected override async Task<bool> OnPreBubbleEventAsync(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
        {
            if (e.Name == DialogEvents.ActivityReceived && dc.Context.Activity.Type == ActivityTypes.Message)
            {
                // Ask parent to perform recognition
                await dc.Parent.EmitEventAsync(AdaptiveEvents.RecognizeUtterance, value: dc.Context.Activity, bubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);

                // Should we allow interruptions
                var canInterrupt = true;
                if (this.AllowInterruptions != null)
                {
                    var (allowInterruptions, error) = this.AllowInterruptions.TryGetValue(dc.State);
                    canInterrupt = error == null && allowInterruptions;
                }

                // Stop bubbling if interruptions ar NOT allowed
                return !canInterrupt;
            }

            return false;
        }

        /// <summary>
        /// Regenerates the <see cref="SkillDialog.DialogOptions"/> based on the values used during the BeingDialog call.
        /// </summary>
        /// <remarks>
        /// The dialog can be resumed in another server or after redeploying the bot, this code ensure that the options used are the ones
        /// used to call BeginDialog.
        /// Also, if ContinueConversation or other methods are called on a server different than the one where BeginDialog was called,
        /// DialogOptions will be empty and this code will make sure it has the right value.
        /// </remarks>
        private void LoadDialogOptions(ITurnContext context, DialogInstance instance)
        {
            var dialogOptions = (SkillDialogOptions)instance.State[_dialogOptionsStateKey];

            DialogOptions.BotId = dialogOptions.BotId;
            DialogOptions.SkillHostEndpoint = dialogOptions.SkillHostEndpoint;
            DialogOptions.ConversationIdFactory = context.TurnState.Get<SkillConversationIdFactoryBase>() ?? throw new NullReferenceException("Unable to locate SkillConversationIdFactoryBase in HostContext");
            DialogOptions.SkillClient = context.TurnState.Get<BotFrameworkClient>() ?? throw new NullReferenceException("Unable to locate BotFrameworkClient in HostContext");
            DialogOptions.ConversationState = context.TurnState.Get<ConversationState>() ?? throw new NullReferenceException($"Unable to get an instance of {nameof(ConversationState)} from TurnState.");
            DialogOptions.ConnectionName = dialogOptions.ConnectionName;

            // Set the skill to call
            DialogOptions.Skill = dialogOptions.Skill;
        }
    }
}
