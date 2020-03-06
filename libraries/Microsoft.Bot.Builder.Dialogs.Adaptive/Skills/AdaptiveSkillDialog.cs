// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Skills
{
    public class AdaptiveSkillDialog : SkillDialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.AdaptiveSkillDialog";

        [JsonConstructor]
        public AdaptiveSkillDialog([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// The default for this will be true, which means the new dialog should not look the activity. You can set this to false to dispatch the activity to the new dialog.
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

        [JsonProperty("botId")]
        public StringExpression BotId { get; set; } = "=settings.MicrosoftAppId";

        [JsonProperty("skillHostEndpoint")]
        public StringExpression SkillHostEndpoint { get; set; } = "=settings.SkillHostEndpoint";

        [JsonProperty("skillAppId")]
        public StringExpression SkillAppId { get; set; }

        [JsonProperty("skillEndpoint")]
        public StringExpression SkillEndpoint { get; set; }

        /// <summary>
        /// Gets or sets template for the activity.
        /// </summary>
        /// <value>
        /// Template for the activity.
        /// </value>
        [JsonProperty("activity")]
        public ITemplate<Activity> Activity { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var dcState = dc.GetState();
            if (Disabled != null && Disabled.GetValue(dcState))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // Update the dialog options with the runtime settings.
            DialogOptions.BotId = BotId.GetValue(dcState);
            DialogOptions.SkillHostEndpoint = new Uri(SkillHostEndpoint.GetValue(dcState));
            DialogOptions.ConversationIdFactory = HostContext.Current.Get<SkillConversationIdFactoryBase>() ?? throw new NullReferenceException("Unable to locate SkillConversationIdFactoryBase in HostContext");
            DialogOptions.SkillClient = HostContext.Current.Get<BotFrameworkClient>() ?? throw new NullReferenceException("Unable to locate BotFrameworkClient in HostContext");
            DialogOptions.ConversationState = dc.Context.TurnState.Get<ConversationState>() ?? throw new NullReferenceException($"Unable to get an instance of {nameof(ConversationState)} from TurnState.");

            // Set the skill to call
            DialogOptions.Skill.Id = DialogOptions.Skill.AppId = SkillAppId.GetValue(dcState);
            DialogOptions.Skill.SkillEndpoint = new Uri(SkillEndpoint.GetValue(dcState));

            // Get the activity to send to the skill.
            var activity = await Activity.BindToData(dc.Context, dcState).ConfigureAwait(false);

            return await base.BeginDialogAsync(dc, new BeginSkillDialogOptions { Activity = activity }, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            var dcState = dc.GetState();

            if (ResultProperty != null)
            {
                dcState.SetValue(ResultProperty.GetValue(dcState), result);
            }

            // By default just end the current dialog and return result to parent.
            return await base.ResumeDialogAsync(dc, reason, result, cancellationToken).ConfigureAwait(false);
        }
    }
}
