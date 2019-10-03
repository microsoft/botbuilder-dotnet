// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChannelPrototype.Controllers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ChannelPrototype.Bots
{
    public class SkillHostBot : ActivityHandler
    {
        private SkillRegistry skillRegistry;
        private ConversationState conversationState;
        private IConfiguration configuration;
        private BotFrameworkHttpAdapter botAdapter;
        private IStatePropertyAccessor<string> activeSkillProperty;

        public SkillHostBot(SkillRegistry skillRegistry, ConversationState conversationState, IConfiguration configuration, BotFrameworkHttpAdapter botAdapter)
        {
            this.skillRegistry = skillRegistry;
            this.conversationState = conversationState;
            this.configuration = configuration;
            this.botAdapter = botAdapter;
            this.activeSkillProperty = conversationState.CreateProperty<string>("activeSkillProperty");
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            // if there is an active skill
            var activeSkillId = await activeSkillProperty.GetAsync(turnContext, () => null);
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
                    await activeSkillProperty.SetAsync(turnContext, "EchoSkill");
                    await conversationState.SaveChangesAsync(turnContext, force: true);

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
            IEventActivity eventActivity = turnContext.Activity.AsEventActivity();

            if (eventActivity.Name == "Skill")
            {
                IConnectorClient client = turnContext.TurnState.Get<IConnectorClient>();
                SkillArgs skillArgs = eventActivity.Value as SkillArgs;
                switch (skillArgs.Method)
                {
                    /// <summary>
                    /// Send activity(activity)
                    /// </summary>
                    /// <summary>
                    /// UpdateActivity(activity)
                    /// </summary>
                    case SkillMethod.SendActivity:
                        var activity = (Activity)skillArgs.Args[0];
                        switch (activity.Type)
                        {
                            case ActivityTypes.Trace:
                            case ActivityTypes.Suggestion:
                            case ActivityTypes.Typing:
                            case ActivityTypes.Message:
                            case ActivityTypes.MessageReaction:
                                skillArgs.Result = await turnContext.SendActivityAsync((Activity)skillArgs.Args[0], cancellationToken);
                                break;

                            case ActivityTypes.EndOfConversation:
                                // process the end of conversation as an end of conversation for the bot to handle
                                {
                                    var botAppId = this.configuration.GetValue<string>("MicrosoftAppId");
                                    var claimsIdentity = new ClaimsIdentity(new List<Claim>
                                    {
                                        // Adding claims for both Emulator and Channel
                                        new Claim(AuthenticationConstants.AudienceClaim, botAppId),
                                        new Claim(AuthenticationConstants.AppIdClaim, botAppId),
                                    });

                                    // map internal activity to incoming context so that UserState is correct for processing the EndOfConversation
                                    activity.ApplyConversationReference(turnContext.Activity.GetConversationReference(), isIncoming: true);

                                    await this.botAdapter.ProcessActivityAsync(claimsIdentity, activity, this.OnTurnAsync, cancellationToken);
                                    skillArgs.Result = new ResourceResponse(id: Guid.NewGuid().ToString("N"));
                                }

                                break;

                            default:
                                // ignore activity
                                break;
                        }

                        break;

                    case SkillMethod.UpdateActivity:
                        skillArgs.Result = await turnContext.SendActivityAsync((Activity)skillArgs.Args[0], cancellationToken);
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

                    if (skillRegistry.ContainsKey(skillId))
                    {
                        // end active skill invocation
                        var activeSkillId = await activeSkillProperty.GetAsync(turnContext, () => null);
                        if (activeSkillId == skillId)
                        {
                            await activeSkillProperty.DeleteAsync(turnContext);
                            await conversationState.SaveChangesAsync(turnContext, force: true);
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
            if (this.skillRegistry.TryGetValue(skillId, out SkillRegistration skillRegistration))
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
