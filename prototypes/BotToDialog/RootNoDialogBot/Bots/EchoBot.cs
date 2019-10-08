// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace RootNoDialogBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly SkillConnector _skillConnector;

        public EchoBot(IConfiguration configuration)
        {
            var skillOptions = new SkillOptions
            {
                Id = configuration["SkillId"],
                Endpoint = new Uri(configuration["SkillAppEndpoint"]),
            };
            var serviceClientCredentials = new MicrosoftAppCredentials(configuration["SkillAppId"], configuration["SkillAppPassword"]);
            _skillConnector = SkillConnectorFactory.Create(skillOptions, serviceClientCredentials, new NullBotTelemetryClient());
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // var ret = await _skillConnector.ProcessActivityAsync(turnContext, turnContext.Activity as Activity, InterceptHandler, cancellationToken);
            var ret = await _skillConnector.ProcessActivityAsync(turnContext, turnContext.Activity as Activity, InterceptHandler, cancellationToken);
            if (ret.Status == SkillTurnStatus.Complete)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("The skill has ended"), cancellationToken);
            }
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

        private static async Task<ResourceResponse[]> InterceptHandler(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            foreach (var activity in activities)
            {
                await turnContext.SendActivityAsync($"Intercept {activity.Type} {activity.Text}");
            }

            // We can return null if we don't want the activity to continue
            return await next().ConfigureAwait(false);
        }
    }
}
