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

        public SkillHostBot(ConversationState conversationState, IConfiguration configuration, BotFrameworkHttpAdapter botAdapter)
        {
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
                await turnContext.Adapter.ForwardActivityAsync(turnContext, activeSkillId, (Activity)turnContext.Activity, cancellationToken);
            }
            else
            {
                if (turnContext.Activity.Text.Contains("skill"))
                {
                    // save conversationRefrence for skill
                    await _activeSkillProperty.SetAsync(turnContext, "EchoSkill");
                    await _conversationState.SaveChangesAsync(turnContext, force: true);

                    // route the activity to the skill
                    await turnContext.Adapter.ForwardActivityAsync(turnContext, "EchoSkill", (Activity)turnContext.Activity, cancellationToken);
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
            if (turnContext.Activity.Type == ActivityTypes.EndOfConversation)
            {
                // TODO probably don't need skillId anymore
                if (activity.Recipient.Properties.ContainsKey("skillId"))
                {
                    var skillId = (string)activity.Recipient.Properties["skillId"];
                    var activeSkillId = await _activeSkillProperty.GetAsync(turnContext, () => null);
                    if (activeSkillId == skillId)
                    {
                        // forget skill invocation
                        await _activeSkillProperty.DeleteAsync(turnContext);
                        await _conversationState.SaveChangesAsync(turnContext, force: true);
                    }

                    return;
                }
            }

            await base.OnUnrecognizedActivityTypeAsync(turnContext, cancellationToken);
        }
    }
}
