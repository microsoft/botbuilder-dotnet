// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Teams.Tests
{
    [TestClass]
    public class TeamsTurnContextExtensionsTests
    {
        [TestMethod]
        public async Task TeamsSendToGeneralChannelAsync()
        {
            // Arrange
            var inboundActivity = new Activity
            {
                Type = ActivityTypes.Message,
                Conversation = new ConversationAccount { Id = "originalId" },
                ChannelData = new TeamsChannelData { Team = new TeamInfo { Id = "team123" } },
            };

            var turnContext = new TurnContext(new SimpleAdapter((Activity[] arg) => { }), inboundActivity);

            var baseUri = new Uri("http://no-where");
            var credentials = new MicrosoftAppCredentials(string.Empty, string.Empty);
            var messageHandler = new RecordingHttpMessageHandler();

            turnContext.TurnState.Add<IConnectorClient>(new ConnectorClient(baseUri, credentials, new HttpClient(messageHandler)));

            // Act
            var (conversationReference, activityId) = await turnContext.TeamsSendToGeneralChannelAsync(MessageFactory.Text("hi"));

            // Assert
            Assert.AreEqual("ConversationId", conversationReference.Conversation.Id);
            Assert.AreEqual("ActivityId", activityId);
        }

        private class RecordingHttpMessageHandler : HttpMessageHandler
        {
            public List<string> Requests { get; } = new List<string>();

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var requestContent = request.Content != null ? await request.Content.ReadAsStringAsync() : "(null)";

                Requests.Add(requestContent);

                var response = new HttpResponseMessage(HttpStatusCode.Created);
                response.Content = new StringContent(new JObject { { "id", "ConversationId" }, { "activityId", "ActivityId" } }.ToString());
                return response;
            }
        }
    }
}
