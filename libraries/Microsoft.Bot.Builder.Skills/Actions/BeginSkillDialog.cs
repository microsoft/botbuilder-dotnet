// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    public class BeginSkillDialog : SkillDialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.BeginSkillDialog";

        [JsonConstructor]
        public BeginSkillDialog([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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

        [JsonProperty("targetSkillAppId")]
        public string TargetSkillAppId { get; set; }

        [JsonProperty("targetSkillEndpoint")]
        public string TargetSkillEndpoint { get; set; }

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
            // TODO: decouple this from config.
            var config = HostContext.Current.Get<IConfiguration>();
            DialogOptions.BotId = config["MicrosoftAppId"];
            DialogOptions.SkillHostEndpoint = new Uri(config["SkillHostEndpoint"]);
            DialogOptions.ConversationIdFactory = HostContext.Current.Get<SkillConversationIdFactoryBase>() ?? throw new NullReferenceException("Unable to locate SkillConversationIdFactoryBase in HostContext");
            DialogOptions.SkillClient = HostContext.Current.Get<BotFrameworkClient>() ?? throw new NullReferenceException("Unable to locate BotFrameworkClient in HostContext");
            DialogOptions.ConversationState = dc.Context.TurnState.Get<ConversationState>();

            DialogOptions.Skill.Id = TargetSkillAppId;
            DialogOptions.Skill.AppId = TargetSkillAppId;
            DialogOptions.Skill.SkillEndpoint = new Uri(TargetSkillEndpoint);

            var activity = await Activity.BindToData(dc.Context, dc.GetState()).ConfigureAwait(false);

            return await base.BeginDialogAsync(dc, new SkillDialogArgs { Activity = activity }, cancellationToken).ConfigureAwait(false);
        }
    }
}
