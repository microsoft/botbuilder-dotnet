// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.using System.Security.Claims;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.IdentityModel.Tokens;
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
        public async Task TenantIdShouldNotFailIfNoChannelData()
        {
            var activity = await ProcessActivity(Channels.Directline, null, null);
            Assert.IsNull(activity.Conversation.TenantId);
        }

        [TestMethod]
        public async Task CreateConversationOverloadProperlySetsTenantId()
        {
            // Arrange
            const string activityIdName = "ActivityId";
            const string activityIdValue = "SendActivityId";
            const string conversationIdName = "Id";
            const string conversationIdValue = "NewConversationId";
            const string tenantIdValue = "theTenantId";
            const string eventActivityName = "CreateConversation";

            Func<Task<HttpResponseMessage>> createResponseMessage = () =>
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                response.Content = new StringContent(new JObject { { activityIdName, activityIdValue }, { conversationIdName, conversationIdValue } }.ToString());
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
                    { ["id"] = tenantIdValue },
                },
                Conversation = new ConversationAccount
                { TenantId = tenantIdValue },
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
            Assert.AreEqual(tenantIdValue, JObject.FromObject(newActivity.ChannelData)["tenant"]["tenantId"]);
            Assert.AreEqual(activityIdValue, newActivity.Id);
            Assert.AreEqual(conversationIdValue, newActivity.Conversation.Id);
            Assert.AreEqual(tenantIdValue, newActivity.Conversation.TenantId);
            Assert.AreEqual(eventActivityName, newActivity.Name);
        }

        [TestMethod]
        public async Task OutgoingActivityIdsAreNotSent()
        {
            // Arrange
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var mockConnector = new MemoryConnectorClient();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var adapter = new BotFrameworkAdapter(mockCredentialProvider.Object, customHttpClient: httpClient);

            var incomingActivity = new Activity("test")
            {
                Id = "testid",
                ChannelId = Channels.Directline,
                ServiceUrl = "https://fake.service.url",
                Conversation = new ConversationAccount
                {
                    Id = "cid",
                }
            };

            var reply = MessageFactory.Text("test");
            reply.Id = "TestReplyId";
            
            // Act
            using (var turnContext = new TurnContext(adapter, incomingActivity))
            {
                turnContext.TurnState.Add<IConnectorClient>(mockConnector);

                var responseIds = await turnContext.SendActivityAsync(reply, default);
            }

            var sentActivity = mockConnector.MemoryConversations.SentActivities.FirstOrDefault(f => f.Type == ActivityTypes.Message);

            // Assert - assert the reply's id is not sent
            Assert.IsNull(sentActivity.Id); 
        }

        [TestMethod]
        public async Task ContinueSkillConversationAsync()
        {
            // Arrange
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var adapter = new BotFrameworkAdapter(mockCredentialProvider.Object, customHttpClient: httpClient);

            // Create ClaimsIdentity that represents Skill2-to-Skill1 communication
            var skill2AppId = "000skill2-aaaa-aaaa-aaaa-1a7454675036";
            var skill1AppId = "000skill1-bbbb-bbbb-bbbb-c8f7cfd51093";

            var skillClaims = new List<Claim>
            {
                new Claim(AuthenticationConstants.AudienceClaim, skill1AppId),
                new Claim(AuthenticationConstants.AppIdClaim, skill2AppId),
                new Claim(AuthenticationConstants.VersionClaim, "1.0")
            };
            var skillsIdentity = new ClaimsIdentity(skillClaims);
            var skill2ServiceUrl = "https://skill2.com/api/skills";

            // Skill1 is calling ContinueSkillConversationAsync() to proactively send an Activity to Skill 2
            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                var adapter = turnContext.Adapter as BotFrameworkAdapter;
                var claimsIdentity = turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey);

                var credsCacheField = typeof(BotFrameworkAdapter).GetField("_appCredentialMap", BindingFlags.NonPublic | BindingFlags.Instance);
                var credsCache = (ConcurrentDictionary<string, AppCredentials>)credsCacheField.GetValue(adapter);
                Assert.AreEqual(1, credsCache.Count);

                // Get AppCredentials for "skill1-to-skill2" communications
                AppCredentials skill2AppCreds;
                credsCache.TryGetValue($"{skill1AppId}{skill2AppId}", out skill2AppCreds);
                Assert.AreEqual(skill1AppId, skill2AppCreds.MicrosoftAppId);
                Assert.AreEqual(skill2AppId, skill2AppCreds.OAuthScope);

                var serviceUri = new Uri(skill2ServiceUrl);
                var clientCacheField = typeof(BotFrameworkAdapter).GetField("_connectorClients", BindingFlags.NonPublic | BindingFlags.Instance);
                var clientCache = (ConcurrentDictionary<string, ConnectorClient>)clientCacheField.GetValue(adapter);
                Assert.AreEqual(1, clientCache.Count);

                // Get "skill1-to-skill2" ConnectorClient
                ConnectorClient toSkill2Client;
                clientCache.TryGetValue($"{skill2ServiceUrl}{skill1AppId}", out toSkill2Client);
                Assert.AreEqual(serviceUri, toSkill2Client.BaseUri);

                // service url and multiple hosted bots in botframeworkadapter?
                var turnStateClient = turnContext.TurnState.Get<IConnectorClient>();

                var clientCreds = turnStateClient.Credentials as AppCredentials;
                Assert.AreEqual(skill2AppId, clientCreds.OAuthScope);
                Assert.AreEqual(skill1AppId, clientCreds.MicrosoftAppId);
            });

            // Create ConversationReference to send a proactive message from Skill1 to Skill2
            var refs = new ConversationReference(serviceUrl: skill2ServiceUrl);

            await adapter.ContinueSkillConversationAsync(skillsIdentity, refs, callback, skill2AppId, default);
        }

        private static async Task<IActivity> ProcessActivity(string channelId, object channelData, string conversationTenantId)
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
                    ChannelData = channelData,
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

        private static async Task<IActivity> ProcessActivity(string channelId, string channelDataTenantId, string conversationTenantId)
        {
            var channelData = new JObject
            {
                ["tenant"] = new JObject
                    { ["id"] = channelDataTenantId },
            };

            return await ProcessActivity(channelId, channelData, conversationTenantId);
        }
    }
}
