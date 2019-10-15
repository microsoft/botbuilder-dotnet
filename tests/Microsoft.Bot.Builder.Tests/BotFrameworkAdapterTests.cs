// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.using System.Security.Claims;

using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class BotFrameworkAdapterTests
    {
        [TestMethod]
        public async Task TenantIdShouldBeSetInConversationForTeams()
        {
            var activity = await ProcessActivity(Channels.Msteams, "theTenantId", null);
            Assert.AreEqual("theTenantId", activity.Conversation.TenantId);
        }

        [TestMethod]
        public async Task TenantIdShouldNotChangeInConversationForTeamsIfPresent()
        {
            var activity = await ProcessActivity(Channels.Msteams, "theTenantId", "shouldNotBeReplaced");
            Assert.AreEqual("shouldNotBeReplaced", activity.Conversation.TenantId);
        }

        [TestMethod]
        public async Task TenantIdShouldNotBeSetInConversationIfNotTeams()
        {
            var activity = await ProcessActivity(Channels.Directline, "theTenantId", null);
            Assert.IsNull(activity.Conversation.TenantId);
        }

        [TestMethod]
        public async Task CreateConversastionOverloadProperlySetsTenantId()
        {
            // Arrange
            const string ActivityIdName = "ActivityId";
            const string ActivityIdValue = "SendActivityId";
            const string ConversationIdName = "Id";
            const string ConversationIdValue = "NewConversationId";
            const string TenantIdValue = "theTenantId";
            const string EventActivityName = "CreateConversation";

            Func<Task<HttpResponseMessage>> createResponseMessage = () =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                response.Content = new StringContent(new JObject { { ActivityIdName, ActivityIdValue }, { ConversationIdName, ConversationIdValue } }.ToString());
                return Task.FromResult(response);
            };

            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => createResponseMessage());

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var adapter = new BotFrameworkAdapter(mockCredentialProvider.Object, customHttpClient: httpClient);

            var activity = new Activity("test")
            {
                ChannelId = Channels.Msteams,
                ServiceUrl = "https://fake.service.url",
                ChannelData = new JObject
                {
                    ["tenant"] = new JObject
                    { ["id"] = TenantIdValue },
                },
                Conversation = new ConversationAccount
                { TenantId = TenantIdValue },
            };

            var parameters = new ConversationParameters()
            {
                Activity = new Activity()
                {
                    ChannelData = activity.ChannelData,
                },
            };
            var reference = activity.GetConversationReference();
            var credentials = new MicrosoftAppCredentials(string.Empty, string.Empty, httpClient);

            Activity newActivity = null;

            Task UpdateParameters(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                newActivity = turnContext.Activity;
                return Task.CompletedTask;
            }

            // Act
            await adapter.CreateConversationAsync(activity.ChannelId, activity.ServiceUrl, credentials, parameters, UpdateParameters, reference, new CancellationToken());

            // Assert - all values set correctly
            Assert.AreEqual(TenantIdValue, JObject.FromObject(newActivity.ChannelData)["tenant"]["tenantId"]);
            Assert.AreEqual(ActivityIdValue, newActivity.Id);
            Assert.AreEqual(ConversationIdValue, newActivity.Conversation.Id);
            Assert.AreEqual(TenantIdValue, newActivity.Conversation.TenantId);
            Assert.AreEqual(EventActivityName, newActivity.Name);
        }

        private static async Task<IActivity> ProcessActivity(string channelId, string channelDataTenantId, string conversationTenantId)
        {
            IActivity activity = null;
            var mockClaims = new Mock<ClaimsIdentity>();
            var mockCredentialProvider = new Mock<ICredentialProvider>();

            var sut = new BotFrameworkAdapter(mockCredentialProvider.Object);
            await sut.ProcessActivityAsync(
                mockClaims.Object,
                new Activity("test")
                {
                    ChannelId = channelId,
                    ServiceUrl = "https://smba.trafficmanager.net/amer/",
                    ChannelData = new JObject
                    {
                        ["tenant"] = new JObject
                        { ["id"] = channelDataTenantId },
                    },
                    Conversation = new ConversationAccount
                    { TenantId = conversationTenantId },
                },
                (context, token) =>
                {
                    activity = context.Activity;
                    return Task.CompletedTask;
                },
                CancellationToken.None);
            return activity;
        }
    }
}
