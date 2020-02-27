// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{
    public class ParentBot : ActivityHandler
    {
        private BotFrameworkHttpClient _client;

        public ParentBot(BotFrameworkHttpClient client)
        {
            _client = client;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("parent: before child"), cancellationToken);

            var activity = MessageFactory.Text("parent to child");
            activity.ApplyConversationReference(turnContext.Activity.GetConversationReference(), true);
            activity.DeliveryMode = DeliveryModes.BufferedReplies;

            var response = await _client.PostActivityAsync<Activity[]>(
                null,
                "toBotId",
                new Uri("http://localhost:3979/api/messages"),
                new Uri("http://tempuri.org/whatever"),
                Guid.NewGuid().ToString(),
                activity,
                cancellationToken);

            if (response.Status == (int)HttpStatusCode.OK)
            {
                await turnContext.SendActivitiesAsync(response.Body, cancellationToken);
            }

            await turnContext.SendActivityAsync(MessageFactory.Text("parent: after child"), cancellationToken);
        }
    }
}
