// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class ReplyToChannelBot : ActivityHandler
    {
        private string _appId;
        private string _appPassword;

        public ReplyToChannelBot(IConfiguration configuration)
        {
            _appId = configuration["MicrosoftAppId"];
            _appPassword = configuration["MicrosoftAppPassword"];
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamsChannelId = turnContext.Activity.TeamsGetChannelId();
            var message = MessageFactory.Text("good morning");
            var creds = new MicrosoftAppCredentials(this._appId, this._appPassword);
            Tuple<ConversationReference, string> tuple = await TeamsInfo.SendMessageToTeamsChannelAsync(turnContext, message, teamsChannelId, creds);

            //// (2) starting a one on one chat

            //var accounts = (await TeamsInfo.GetMembersAsync(turnContext)).ToArray();
            //var account = accounts.First();

            //var conversationParameters = new ConversationParameters
            //{
            //    IsGroup = false,
            //    Bot = turnContext.Activity.Recipient,
            //    Members = new ChannelAccount[] { account },
            //    TenantId = turnContext.Activity.Conversation.TenantId,
            //};

            //ConversationReference conversationReference = null;

            //await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(
            //    teamsChannelId,
            //    serviceUrl,
            //    credentials,
            //    conversationParameters,
            //    (t, ct) =>
            //    {
            //        conversationReference = t.Activity.GetConversationReference();
            //        return Task.CompletedTask;
            //    },
            //    cancellationToken);

            await ((BotFrameworkAdapter)turnContext.Adapter).ContinueConversationAsync(
                this._appId,
                tuple.Item1,
                async (t, ct) =>
                {
                    await t.SendActivityAsync(MessageFactory.Text($"this will be the first reply. The Activity.Id of the top level message is {tuple.Item2}"), ct);
                },
                cancellationToken);
        }
    }
}
