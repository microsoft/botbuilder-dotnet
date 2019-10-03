// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using SkillHost.Controllers;

namespace SkillHost.Bots
{
    public class SkillHostBot : ActivityHandler
    {
        private readonly IStatePropertyAccessor<string> _activeSkillProperty;
        private readonly BotFrameworkHttpAdapter _botAdapter;
        private readonly IConfiguration _configuration;
        private readonly ConversationState _conversationState;
        private readonly SkillRegistry _skillRegistry;

        public SkillHostBot(SkillRegistry skillRegistry, ConversationState conversationState, IConfiguration configuration, BotFrameworkHttpAdapter botAdapter)
        {
            this._skillRegistry = skillRegistry;
            this._conversationState = conversationState;
            this._configuration = configuration;
            this._botAdapter = botAdapter;
            this._activeSkillProperty = conversationState.CreateProperty<string>("activeSkillProperty");
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            // if there is an active skill
            var activeSkillId = await _activeSkillProperty.GetAsync(turnContext, () => null);
            if (activeSkillId != null)
            {
                // route activity to the skill
                await this.ForwardActivityToSkill(activeSkillId, turnContext.Activity, cancellationToken);
            }
            else
            {
                if (turnContext.Activity.Text.Contains("skill"))
                {
                    // save conversationRefrence for skill
                    await _activeSkillProperty.SetAsync(turnContext, "EchoSkill");
                    await _conversationState.SaveChangesAsync(turnContext, force: true);

                    // route the activity to the skill
                    await this.ForwardActivityToSkill("EchoSkill", turnContext.Activity, cancellationToken);
                }
                else
                {
                    // just responsd
                    await turnContext.SendActivityAsync(MessageFactory.Text($"me no nothin'"), cancellationToken);
                }
            }
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            var eventActivity = turnContext.Activity.AsEventActivity();

            if (eventActivity.Name == "Skill")
            {
                var client = turnContext.TurnState.Get<IConnectorClient>();
                var skillArgs = eventActivity.Value as SkillArgs;
                switch (skillArgs.Method)
                {
                    /// <summary>
                    /// Send activity(conversationId, activity)
                    /// </summary>
                    /// <summary>
                    /// UpdateActivity(conversationId, activity)
                    /// </summary>
                    case SkillMethod.SendActivity:
                        var activity = (Activity)skillArgs.Args[1];

                        switch (activity.Type)
                        {
                            case ActivityTypes.Trace:
                            case ActivityTypes.Suggestion:
                            case ActivityTypes.Typing:
                            case ActivityTypes.Message:
                            case ActivityTypes.MessageReaction:
                                skillArgs.Result = await turnContext.SendActivityAsync(activity, cancellationToken);
                                break;

                            case ActivityTypes.EndOfConversation:
                                // process the end of conversation as an end of conversation for the bot to handle
                                var botAppId = this._configuration.GetValue<string>("MicrosoftAppId");
                                var claimsIdentity = new ClaimsIdentity(new List<Claim>
                                {
                                    // Adding claims for both Emulator and Channel
                                    new Claim(AuthenticationConstants.AudienceClaim, botAppId),
                                    new Claim(AuthenticationConstants.AppIdClaim, botAppId),
                                });

                                // map internal activity to incoming context so that UserState is correct for processing the EndOfConversation
                                var from = activity.From;
                                activity.From = activity.Recipient;
                                activity.Recipient = from;

                                await this._botAdapter.ProcessActivityAsync(claimsIdentity, activity, this.OnTurnAsync, cancellationToken);
                                skillArgs.Result = new ResourceResponse(id: Guid.NewGuid().ToString("N"));

                                break;

                            default:
                                // ignore activity
                                break;
                        }

                        break;

                    case SkillMethod.UpdateActivity:
                        skillArgs.Result = await turnContext.SendActivityAsync((Activity)skillArgs.Args[1], cancellationToken);
                        break;

                    /// <summary>
                    /// DeleteActivity(conversationId, activityId)
                    /// </summary>
                    case SkillMethod.DeleteActivity:
                        await turnContext.DeleteActivityAsync((string)skillArgs.Args[1], cancellationToken);
                        break;

                    /// <summary>
                    /// SendConversationHistory(conversationId, history)
                    /// </summary>
                    case SkillMethod.SendConversationHistory:
                        skillArgs.Result = await client.Conversations.SendConversationHistoryAsync((string)skillArgs.Args[0], (Transcript)skillArgs.Args[1], cancellationToken);
                        break;

                    /// <summary>
                    /// GetConversationMembers(conversationId)
                    /// </summary>
                    case SkillMethod.GetConversationMembers:
                        skillArgs.Result = await client.Conversations.GetConversationMembersAsync((string)skillArgs.Args[0], cancellationToken);
                        break;

                    /// <summary>
                    /// GetConversationPageMembers(conversationId, (int)pageSize, continuationToken)
                    /// </summary>
                    case SkillMethod.GetConversationPagedMembers:
                        skillArgs.Result = await client.Conversations.GetConversationPagedMembersAsync((string)skillArgs.Args[0], (int)skillArgs.Args[1], (string)skillArgs.Args[2], cancellationToken);
                        break;

                    /// <summary>
                    /// DeleteConversationMember(conversationId, memberId)
                    /// </summary>
                    case SkillMethod.DeleteConversationMember:
                        await client.Conversations.DeleteConversationMemberAsync((string)skillArgs.Args[0], (string)skillArgs.Args[1], cancellationToken);
                        break;

                    /// <summary>
                    /// GetActivityMembers(conversationId, activityId)
                    /// </summary>
                    case SkillMethod.GetActivityMembers:
                        skillArgs.Result = await client.Conversations.GetActivityMembersAsync((string)skillArgs.Args[0], (string)skillArgs.Args[1], cancellationToken);
                        break;

                    /// <summary>
                    /// UploadAttachment(conversationId, attachmentData)
                    /// </summary>
                    case SkillMethod.UploadAttachment:
                        skillArgs.Result = await client.Conversations.UploadAttachmentAsync((string)skillArgs.Args[0], (AttachmentData)skillArgs.Args[1], cancellationToken);
                        break;
                }
            }
            else
            {
                await base.OnEventActivityAsync(turnContext, cancellationToken);
            }
        }

        protected override async Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            if (turnContext.Activity.Type == ActivityTypes.EndOfConversation)
            {
                // TODO probably don't need skillId anymore
                if (activity.Recipient.Properties.ContainsKey("skillId"))
                {
                    var skillId = (string)activity.Recipient.Properties["skillId"];

                    if (_skillRegistry.ContainsKey(skillId))
                    {
                        // end active skill invocation
                        var activeSkillId = await _activeSkillProperty.GetAsync(turnContext, () => null);
                        if (activeSkillId == skillId)
                        {
                            await _activeSkillProperty.DeleteAsync(turnContext);
                            await _conversationState.SaveChangesAsync(turnContext, force: true);
                        }

                        return;
                    }
                }
            }

            await base.OnUnrecognizedActivityTypeAsync(turnContext, cancellationToken);
        }

        private async Task ForwardActivityToSkill(string skillId, IActivity activity, CancellationToken cancellationToken)
        {
            // route the activity to the skill
            if (this._skillRegistry.TryGetValue(skillId, out var skillRegistration))
            {
                // TODO probably don't need skillId anymore
                activity.Recipient.Properties["skillId"] = skillId;
                activity.Conversation.Id = SkillHostController.GetSkillConversationId(activity);
                activity.ServiceUrl = "http://localhost:3978/";

                // send it
                var client = new HttpClient();

                // TODO add client authorization header
                var response = await client.PostAsJsonAsync($"{skillRegistration.ServiceUrl}", activity, cancellationToken);
            }
            else
            {
                throw new ArgumentException($"Skill:{skillId} isn't registered as a registered skill");
            }
        }
    }
}
