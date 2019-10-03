// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ChannelPrototype.Controllers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace ChannelPrototype.Bots
{
    public class SkillHostBot : ActivityHandler
    {
        private SkillRegistry skillRegistry;
        private IStorage storage;
        private ConversationState conversationState;
        private IConfiguration configuration;
        private BotFrameworkHttpAdapter botAdapter;
        private IStatePropertyAccessor<string> activeSkillProperty;

        public SkillHostBot(SkillRegistry skillRegistry, IStorage storage, ConversationState conversationState, IConfiguration configuration, BotFrameworkHttpAdapter botAdapter)
        {
            this.storage = storage;
            this.skillRegistry = skillRegistry;
            this.conversationState = conversationState;
            this.configuration = configuration;
            this.botAdapter = botAdapter;
            this.activeSkillProperty = conversationState.CreateProperty<string>("activeSkillProperty");
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            // If from has a skillId, then it's a message from the Skill -> Bot -> user
            if (activity.Recipient.Properties.ContainsKey("skillId"))
            {
                var skillId = (string)activity.Recipient.Properties["skillId"];

                if (skillRegistry.ContainsKey(skillId))
                {
                    // this is an activity from a known skill, route to user
                    var resource = await turnContext.SendActivityAsync(activity);

                    // the SkillHostController sets the activity.id as a way of passing the result back to the SkillHostController.
                    activity.Id = resource.Id;
                    return;
                }
                else
                {
                    // just ignore it
                    return;
                }
            }

            // then this is a message from the user

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

        protected override async Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            if (activity.Recipient.Properties.ContainsKey("skillId"))
            {
                var skillId = (string)activity.Recipient.Properties["skillId"];

                if (skillRegistry.ContainsKey(skillId))
                {
                    ResourceResponse resource;
                    switch (turnContext.Activity.Type)
                    {
                        case ActivityTypes.MessageReaction:
                        case ActivityTypes.Typing:
                        case ActivityTypes.Trace:
                            resource = await turnContext.SendActivityAsync(activity);
                            activity.Id = resource.Id;
                            return;

                        case ActivityTypes.MessageUpdate:
                            resource = await turnContext.UpdateActivityAsync((Activity)activity.Value, cancellationToken);
                            activity.Id = resource.Id;
                            return;

                        case ActivityTypes.MessageDelete:
                            await turnContext.DeleteActivityAsync(activity.Id, cancellationToken);
                            return;

                        case ActivityTypes.EndOfConversation:
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
                activity.Recipient.Properties["skillId"] = skillId;
                if (!activity.Conversation.Properties.ContainsKey("serviceUrl"))
                {
                    activity.Conversation.Properties["serviceUrl"] = activity.ServiceUrl;
                    activity.ServiceUrl = "http://localhost:3978/";
                }

                // send it
                HttpClient client = new HttpClient();
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
