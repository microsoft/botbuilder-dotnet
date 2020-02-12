// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Expressions.Properties;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    public class BeginSkillDialog : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.BeginSkillDialog";

        private readonly string _botId;
        private readonly SkillConversationIdFactoryBase _conversationIdFactory;
        private readonly BotFrameworkClient _skillClient;
        private readonly string _skillHostEndpoint;

        [JsonConstructor]
        public BeginSkillDialog([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            RegisterSourceLocation(callerPath, callerLine);
            _skillClient = HostContext.Current.Get<BotFrameworkClient>() ?? throw new NullReferenceException("Unable to locate BotFrameworkClient in HostContext");
            _conversationIdFactory = HostContext.Current.Get<SkillConversationIdFactoryBase>() ?? throw new NullReferenceException("Unable to locate SkillConversationIdFactoryBase in HostContext");
            
            // TODO: decouple this from config.
            var config = HostContext.Current.Get<IConfiguration>();
            _botId = config["MicrosoftAppId"];
            _skillHostEndpoint = config["SkillHostEndpoint"];
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

        [JsonProperty("targetSkillSkillEndpoint")]
        public string TargetSkillSkillEndpoint { get; set; }

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
            // TODO: add validations

            var skillInfo = new BotFrameworkSkill
            {
                Id = TargetSkillAppId,
                AppId = TargetSkillAppId,
                SkillEndpoint = new Uri(TargetSkillSkillEndpoint)
            };

            var convoState = dc.Context.TurnState.Get<ConversationState>();

            // Store Skill information for this dialog instance
            var activeSkillProperty = convoState.CreateProperty<BotFrameworkSkill>($"{typeof(SkillDialog).FullName}.ActiveSkillProperty");
            await activeSkillProperty.SetAsync(dc.Context, skillInfo, cancellationToken).ConfigureAwait(false);

            var activity = await Activity.BindToData(dc.Context, dc.GetState()).ConfigureAwait(false);
            await dc.Context.TraceActivityAsync($"{GetType().Name}.BeginDialogAsync()", label: $"Using activity of type: {activity.Type}", cancellationToken: cancellationToken).ConfigureAwait(false);
            if (activity.Type != ActivityTypes.Message && activity.Type != ActivityTypes.Event)
            {
                throw new ArgumentException($"Invalid activity type {activity.Type} in {nameof(Activity)} property");
            }

            ApplyParentActivityProperties(dc, activity);
            await SendToSkillAsync(dc, activity, skillInfo, cancellationToken).ConfigureAwait(false);
            return EndOfTurn;
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            await dc.Context.TraceActivityAsync($"{GetType().Name}.ContinueDialogAsync()", label: $"ActivityType: {dc.Context.Activity.Type}", cancellationToken: cancellationToken).ConfigureAwait(false);

            var convoState = dc.Context.TurnState.Get<ConversationState>();

            // Store Skill information for this dialog instance
            var activeSkillProperty = convoState.CreateProperty<BotFrameworkSkill>($"{typeof(SkillDialog).FullName}.ActiveSkillProperty");
            var skillInfo = await activeSkillProperty.GetAsync(dc.Context, () => null, cancellationToken).ConfigureAwait(false);

            if (dc.Context.Activity.Type == ActivityTypes.EndOfConversation)
            {
                await dc.Context.TraceActivityAsync($"{GetType().Name}.ContinueDialogAsync()", label: "Got EndOfConversation", cancellationToken: cancellationToken).ConfigureAwait(false);
                return await dc.EndDialogAsync(dc.Context.Activity.Value, cancellationToken).ConfigureAwait(false);
            }

            if (dc.Context.Activity.Type == ActivityTypes.Message || dc.Context.Activity.Type == ActivityTypes.Event)
            {
                // Just forward to the remote skill
                await SendToSkillAsync(dc, dc.Context.Activity, skillInfo, cancellationToken).ConfigureAwait(false);
            }

            return EndOfTurn;
        }

        private static void ApplyParentActivityProperties(DialogContext dc, Activity skillActivity)
        {
            // Apply conversation reference and common properties from incoming activity before sending.
            skillActivity.ApplyConversationReference(dc.Context.Activity.GetConversationReference(), true);

            // skillActivity.Value = dialogArgs.Value;
            // skillActivity.ChannelData = dc.Context.Activity.ChannelData;
            // skillActivity.Properties = dc.Context.Activity.Properties;
        }

        private async Task<InvokeResponse> SendToSkillAsync(DialogContext dc, Activity activity, BotFrameworkSkill skillInfo, CancellationToken cancellationToken)
        {
            // Always save state before forwarding
            // (the dialog stack won't get updated with the skillDialog and things won't work if you don't)
            await dc.Context.TurnState.Get<ConversationState>().SaveChangesAsync(dc.Context, true, cancellationToken).ConfigureAwait(false);

            var skillConversationId = await _conversationIdFactory.CreateSkillConversationIdAsync(activity.GetConversationReference(), cancellationToken).ConfigureAwait(false);
            var response = await _skillClient.PostActivityAsync(_botId, skillInfo.AppId, skillInfo.SkillEndpoint, new Uri(_skillHostEndpoint), skillConversationId, activity, cancellationToken).ConfigureAwait(false);
            if (!(response.Status >= 200 && response.Status <= 299))
            {
                throw new HttpRequestException($"Error invoking the skill id: \"{skillInfo.Id}\" at \"{skillInfo.SkillEndpoint}\" (status is {response.Status}). \r\n {response.Body}");
            }

            return response;
        }
    }
}
