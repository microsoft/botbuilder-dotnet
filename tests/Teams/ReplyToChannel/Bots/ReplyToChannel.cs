// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class ActivityUpdateAndDeleteBot : ActivityHandler
    {
        private string _botId;

        public ActivityUpdateAndDeleteBot(IConfiguration configuration)
        {
            _botId = configuration["MicrosoftAppId"];
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamChannelId = turnContext.Activity.TeamsGetChannelId();
            var message = MessageFactory.Text("good morning");

            var (conversationReference, activityId) = await turnContext.TeamsCreateConversationAsync(teamChannelId, message, cancellationToken);

            await ((BotFrameworkAdapter)turnContext.Adapter).ContinueConversationAsync(
                _botId,
                conversationReference,
                async (t, ct) =>
                {
                    await t.SendActivityAsync(MessageFactory.Text("good afternoon"), ct);
                    await t.SendActivityAsync(MessageFactory.Text("good night"), ct);
                },
                cancellationToken);
        }
    }
}
