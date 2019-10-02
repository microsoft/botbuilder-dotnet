// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ChannelPrototype.Controllers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;

namespace ChannelPrototype.Bots
{
    public class SkillHostBot : ActivityHandler
    {
        private SkillRegistry skillRegistry;
        private IStorage storage;
        private ConversationState conversationState;
        private IConfiguration configuration;
        private BotAdapter botAdapter;
        private IStatePropertyAccessor<ConversationReference> skillConversationProperty;

        public SkillHostBot(SkillRegistry skillRegistry, IStorage storage, ConversationState conversationState, IConfiguration configuration, BotAdapter botAdapter)
        {
            this.storage = storage;
            this.skillRegistry = skillRegistry;
            this.conversationState = conversationState;
            this.configuration = configuration;
            this.botAdapter = botAdapter;
            this.skillConversationProperty = conversationState.CreateProperty<ConversationReference>("skillConversationProperty");
        }


        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var botAppId = configuration.GetValue<string>("MicrosoftAppId");
            var skillConversationReference = await skillConversationProperty.GetAsync(turnContext, () => null);

            // if there is skillConversationReference
            if (skillConversationReference != null)
            {
                await SendActivityToSkill(skillConversationReference, turnContext.Activity, cancellationToken);
            }
            else
            {
                if (turnContext.Activity.Text.Contains("skill"))
                {
                    var originalConversationReference = turnContext.Activity.GetConversationReference();

                    skillConversationReference = await StartSkillConversation(botAppId, "EchoSkill", originalConversationReference, cancellationToken);

                    // save conversationRefrence for skill
                    await skillConversationProperty.SetAsync(turnContext, skillConversationReference);
                    await conversationState.SaveChangesAsync(turnContext);

                    await SendActivityToSkill(skillConversationReference, turnContext.Activity, cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"me no nothin'"), cancellationToken);
                }
            }
        }

        protected override async Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.EndOfConversation)
            {
                // if it is an end of conversation for a conversation that is in our skills table
                var originalConversationReference = await GetOriginalConversationReference(turnContext, turnContext.Activity.Conversation.Id);
                if (originalConversationReference != null)
                {
                    await DeleteSkillConversation(turnContext.Activity.Conversation.Id);

                    var botAppId = configuration.GetValue<string>("MicrosoftAppId");
                    await this.botAdapter.ContinueConversationAsync(botAppId, originalConversationReference, async (originalContext, token) =>
                    {
                        await this.skillConversationProperty.DeleteAsync(originalContext);
                        await this.conversationState.SaveChangesAsync(originalContext, force: true);
                    }, cancellationToken);
                    return;
                }
            }
            await base.OnUnrecognizedActivityTypeAsync(turnContext, cancellationToken);
        }


        private async Task<ConversationReference> StartSkillConversation(String botAppId, string skillName, ConversationReference originalConversationReference, CancellationToken cancellationToken)
        {
            var skillConversationId = await CreateSkillConversationId(skillName, originalConversationReference);
            var skillConversationReference = new ConversationReference()
            {
                ChannelId = originalConversationReference.ChannelId,
                ServiceUrl = "http://localhost:3978/", // url of the skill host adapter implementing /api/conversations 
                User = originalConversationReference.User,
                Bot = new ChannelAccount(id: skillName, role: RoleTypes.Bot),
                Conversation = new ConversationAccount(role: "skill", id: skillConversationId)
            };

            // Send ConversationUpdate
            //var activity = Activity.CreateConversationUpdateActivity();
            //activity.Id = Guid.NewGuid().ToString("N");
            //activity.MembersAdded.Add(skillConversationReference.User);
            //activity.MembersAdded.Add(skillConversationReference.Bot);
            //await SendSkillActivity(botAppId, skillConversationReference, activity, cancellationToken);
            return skillConversationReference;
        }

        private async Task SendActivityToSkill(ConversationReference skillConversationReference, IActivity activity, CancellationToken cancellationToken)
        {
            // route the activity to the skill

            // clone activity
            var newActivity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
            newActivity.Id = Guid.NewGuid().ToString("N");

            // stamp it with conversationReference
            newActivity.ApplyConversationReference(skillConversationReference, true);

            // set RelatesTo so it knows the original relates to
            newActivity.RelatesTo = activity.GetConversationReference();

            if (this.skillRegistry.TryGetValue(skillConversationReference.Bot.Id, out SkillRegistration skillRegistration))
            {
                // send it
                HttpClient client = new HttpClient();
                // TODO add client authorization header
                var response = await client.PostAsJsonAsync($"{skillRegistration.ServiceUrl}", newActivity, cancellationToken);
            }
            else
            {
                throw new ArgumentException($"Skill:{skillConversationReference.Bot.Id} isn't registered as a valid skill");
            }
        }

        /// <summary>
        /// Create new SkillConversationId
        /// </summary>
        /// <param name="originalConversationReference">reference to save</param>
        /// <returns>new skillConversationId</returns>
        public async Task<string> CreateSkillConversationId(string skillId, ConversationReference originalConversationReference)
        {
            if (this.skillRegistry.ContainsKey(skillId))
            {
                var skillConversationId = Guid.NewGuid().ToString("N");
                var changes = new Dictionary<string, object>()
                {
                    { $"skill/{skillConversationId}", originalConversationReference}
                };

                await this.storage.WriteAsync(changes);
                return skillConversationId;
            }
            throw new ArgumentException($"{skillId} is not a regsitered skill");
        }

        /// <summary>
        /// GET original conversationReference 
        /// </summary>
        /// <param name="skillConversationId"></param>
        /// <returns></returns>
        public async Task<ConversationReference> GetOriginalConversationReference(ITurnContext context, string skillConversationId)
        {
            string key = $"skill/{skillConversationId}";
            var result = await this.storage.ReadAsync(new string[] { key });
            if (result != null)
            {
                return JsonConvert.DeserializeObject<ConversationReference>(JsonConvert.SerializeObject(result[key]));
            }
            return null;
        }

        /// <summary>
        /// DELETE skill conversation
        /// </summary>
        /// <param name="skillConversationId"></param>
        /// <returns></returns>
        public async Task DeleteSkillConversation(string skillConversationId)
        {
            await this.storage.DeleteAsync(new string[] { $"skill/{skillConversationId}" });
        }

    }
}
