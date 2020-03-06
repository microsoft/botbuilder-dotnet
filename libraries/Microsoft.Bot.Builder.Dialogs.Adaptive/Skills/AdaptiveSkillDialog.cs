// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
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
            // Update the dialog options with the runtime settings.
            DialogOptions.BotId = BotId.GetValue(dc.GetState());
            DialogOptions.SkillHostEndpoint = new Uri(SkillHostEndpoint.GetValue(dc.GetState()));
            DialogOptions.ConversationIdFactory = HostContext.Current.Get<SkillConversationIdFactoryBase>() ?? throw new NullReferenceException("Unable to locate SkillConversationIdFactoryBase in HostContext");
            DialogOptions.SkillClient = HostContext.Current.Get<BotFrameworkClient>() ?? throw new NullReferenceException("Unable to locate BotFrameworkClient in HostContext");
            DialogOptions.ConversationState = dc.Context.TurnState.Get<ConversationState>();

            // Set the skill to call
            DialogOptions.Skill.Id = SkillAppId.GetValue(dc.GetState());
            DialogOptions.Skill.AppId = SkillAppId.GetValue(dc.GetState());
            DialogOptions.Skill.SkillEndpoint = new Uri(SkillEndpoint.GetValue(dc.GetState()));

            // Get the activity to send to the skill.
            var activity = await Activity.BindToData(dc.Context, dc.GetState()).ConfigureAwait(false);

            return await base.BeginDialogAsync(dc, new BeginSkillDialogOptions { Activity = activity }, cancellationToken).ConfigureAwait(false);
        }
    }
}
