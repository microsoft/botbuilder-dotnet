// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace SimpleRootBot.Bots
{
    public class RootBot : ActivityHandler
    {
        private readonly IStatePropertyAccessor<string> _activeSkillProperty;
        private readonly ConversationState _conversationState;
        private readonly SkillRegistry _skillRegistry;

        public RootBot(SkillRegistry skillRegistry, ConversationState conversationState)
        {
            _skillRegistry = skillRegistry;
            _conversationState = conversationState;
            _activeSkillProperty = conversationState.CreateProperty<string>("activeSkillProperty");
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            // If from has a skillId, then it's a message from the Skill -> Bot -> user
            if (activity.Recipient.Properties.ContainsKey("skillId"))
            {
                var skillId = (string)activity.Recipient.Properties["skillId"];

                if (_skillRegistry.ContainsKey(skillId))
                {
                    // this is an activity from a known skill, route to user
                    var resource = await turnContext.SendActivityAsync(activity, cancellationToken);

                    // the SkillHostController sets the activity.id as a way of passing the result back to the SkillHostController.
                    activity.Id = resource.Id;
                    return;
                }

                // just ignore it
                return;
            }

            // then this is a message from the user

            // if there is an active skill
            var activeSkillId = await _activeSkillProperty.GetAsync(turnContext, () => null, cancellationToken);
            if (activeSkillId != null)
            {
                // route activity to the skill
                await ForwardActivityToSkill(activeSkillId, turnContext.Activity, cancellationToken);
            }
            else
            {
                if (turnContext.Activity.Text.Contains("skill"))
                {
                    // save conversationReference for skill
                    await _activeSkillProperty.SetAsync(turnContext, "EchoSkill", cancellationToken);
                    await _conversationState.SaveChangesAsync(turnContext, force: true, cancellationToken: cancellationToken);

                    // route the activity to the skill
                    await ForwardActivityToSkill("EchoSkill", turnContext.Activity, cancellationToken);
                }
                else
                {
                    // just respond
                    await turnContext.SendActivityAsync(MessageFactory.Text("Me no nothin'. Say \"skill\" and I'll patch you through"), cancellationToken);
                }
            }
        }

        protected override async Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            if (activity.Recipient.Properties.ContainsKey("skillId"))
            {
                var skillId = (string)activity.Recipient.Properties["skillId"];

                if (_skillRegistry.ContainsKey(skillId))
                {
                    ResourceResponse resource;
                    switch (turnContext.Activity.Type)
                    {
                        case ActivityTypes.MessageReaction:
                        case ActivityTypes.Typing:
                        case ActivityTypes.Trace:
                            resource = await turnContext.SendActivityAsync(activity, cancellationToken);
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
                            var activeSkillId = await _activeSkillProperty.GetAsync(turnContext, () => null, cancellationToken);
                            if (activeSkillId == skillId)
                            {
                                await _activeSkillProperty.DeleteAsync(turnContext, cancellationToken);
                                await _conversationState.SaveChangesAsync(turnContext, force: true, cancellationToken: cancellationToken);
                            }

                            // We are back
                            await turnContext.SendActivityAsync(MessageFactory.Text("Back in the root bot. Say \"skill\" and I'll patch you through"), cancellationToken);
                            return;
                    }
                }
            }

            await base.OnUnrecognizedActivityTypeAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello and welcome!"), cancellationToken);
                }
            }
        }

        private async Task ForwardActivityToSkill(string skillId, IActivity activity, CancellationToken cancellationToken)
        {
            // route the activity to the skill
            if (_skillRegistry.TryGetValue(skillId, out var skillRegistration))
            {
                activity.Recipient.Properties["skillId"] = skillId;
                if (!activity.Conversation.Properties.ContainsKey("serviceUrl"))
                {
                    activity.Conversation.Properties["serviceUrl"] = activity.ServiceUrl;
                    activity.ServiceUrl = "http://localhost:3978/";
                }

                // send it
                using (var client = new HttpClient())
                {
                    // TODO add client authorization header
                    var response = await client.PostAsJsonAsync($"{skillRegistration.ServiceUrl}", activity, cancellationToken);
                }
            }
            else
            {
                throw new ArgumentException($"Skill:{skillId} isn't registered as a registered skill");
            }
        }
    }
}
