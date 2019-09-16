// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class RosterBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            var teamsConnectorClient = new TeamsConnectorClient(connectorClient.Credentials);
            teamsConnectorClient.BaseUri = new Uri(turnContext.Activity.ServiceUrl);

            turnContext.Activity.RemoveRecipientMention();

            switch (turnContext.Activity.Text)
            {
                case "show team members":
                    await ShowTeamMembers(turnContext, teamsConnectorClient, cancellationToken);
                    break;

                case "show group chat members":
                    await ShowGroupChatMembers(turnContext, teamsConnectorClient, cancellationToken);
                    break;

                case "showchannels":
                case "show channels":
                    await ShowChannels(turnContext, teamsConnectorClient, cancellationToken);
                    break;

                case "show team details":
                    await ShowTeamDetails(turnContext, teamsConnectorClient, cancellationToken);
                    break;

                default:
                    await turnContext.SendActivityAsync("Invalid command. Type \"Show channels\" to see a channel list. Type \"Show members\" to see a list of members in a team. " +
                        "Type \"show group chat members\" to see members in a group chat.");
                    break;
            }
        }

        private async Task ShowTeamDetails(ITurnContext<IMessageActivity> turnContext, TeamsConnectorClient teamsConnectorClient, CancellationToken cancellationToken)
        {
            var teamsDetails = await teamsConnectorClient.Teams.FetchTeamDetailsAsync(turnContext.Activity.GetChannelData<TeamsChannelData>().Team.Id);

            var replyActivity = MessageFactory.Text($"The team name is {teamsDetails.Name}. The team ID is {teamsDetails.Id}. The ADDGroupID is {teamsDetails.AadGroupId}.");

            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        private Task ShowTeamMembers(ITurnContext<IMessageActivity> turnContext, TeamsConnectorClient teamsConnectorClient, CancellationToken cancellationToken)
            => ShowMembers(turnContext, teamsConnectorClient, turnContext.Activity.GetChannelData<TeamsChannelData>().Team.Id, cancellationToken);

        private Task ShowGroupChatMembers(ITurnContext<IMessageActivity> turnContext, TeamsConnectorClient teamsConnectorClient, CancellationToken cancellationToken)
            => ShowMembers(turnContext, teamsConnectorClient, turnContext.Activity.Conversation.Id, cancellationToken);

        private async Task ShowChannels(ITurnContext<IMessageActivity> turnContext, TeamsConnectorClient teamsConnectorClient, CancellationToken cancellationToken)
        {
            var channelList = await teamsConnectorClient.Teams.FetchChannelListAsync(turnContext.Activity.GetChannelData<TeamsChannelData>().Team.Id);

            var replyActivity = MessageFactory.Text($"<at>{turnContext.Activity.From.Name}</at> Total of {channelList.Conversations.Count} channels are currently in team");
            replyActivity.Entities = new List<Entity> { new Mention { Text = turnContext.Activity.From.Name, Mentioned = turnContext.Activity.From } };

            await turnContext.SendActivityAsync(replyActivity);

            var messages = channelList.Conversations.Select(channel => $"{channel.Id} --> {channel.Name}");

            await SendInBatches(turnContext, messages, cancellationToken);
        }

        private async Task ShowMembers(ITurnContext<IMessageActivity> turnContext, TeamsConnectorClient teamsConnectorClient, string conversationId, CancellationToken cancellationToken)
        {
            var teamMembers = await turnContext.TurnState.Get<IConnectorClient>().Conversations.GetConversationMembersAsync(conversationId);

            var replyActivity = MessageFactory.Text($"<at>{turnContext.Activity.From.Name}</at> Total of {teamMembers.Count} members are currently in team");
            replyActivity.Entities = new List<Entity> { new Mention { Text = turnContext.Activity.From.Name, Mentioned = turnContext.Activity.From } };

            await turnContext.SendActivityAsync(replyActivity);

            var messages = teamMembers
                .Select(channelAccount => JObject.FromObject(channelAccount).ToObject<TeamsChannelAccount>())
                .Select(teamsChannelAccount => $"{teamsChannelAccount.AadObjectId} --> {teamsChannelAccount.Name} -->  {teamsChannelAccount.UserPrincipalName}");

            await SendInBatches(turnContext, messages, cancellationToken);
        }

        private static async Task SendInBatches(ITurnContext<IMessageActivity> turnContext, IEnumerable<string> messages, CancellationToken cancellationToken)
        {
            var batch = new List<string>();
            foreach (var msg in messages)
            {
                batch.Add(msg);

                if (batch.Count == 10)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(string.Join("<br>", batch)), cancellationToken);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(string.Join("<br>", batch)), cancellationToken);
            }
        }
    }
}
