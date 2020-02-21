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
        private const string CredsCacheName = "_appCredentialMap";
        private const string ClientsCacheName = "_connectorClients";

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
        public async Task ProcessActivityAsyncCreatesCorrectCredsAndClient()
        {
            var botAppId = "00000000-0000-0000-0000-000000000001";
            var claims = new List<Claim>
            {
                new Claim(AuthenticationConstants.AudienceClaim, botAppId),
                new Claim(AuthenticationConstants.AppIdClaim, botAppId),
                new Claim(AuthenticationConstants.VersionClaim, "1.0")
            };
            var identity = new ClaimsIdentity(claims);

            var credentialProvider = new SimpleCredentialProvider() { AppId = botAppId };
            var serviceUrl = "https://smba.trafficmanager.net/amer/";
            var callback = new BotCallbackHandler(async (context, ct) =>
            {
                GetCredsAndAssertValues(context, botAppId, AuthenticationConstants.ToChannelFromBotOAuthScope, 1);
                GetClientAndAssertValues(
                    context,
                    botAppId,
                    AuthenticationConstants.ToChannelFromBotOAuthScope,
                    new Uri(serviceUrl),
                    1);

                var scope = context.TurnState.Get<string>(BotAdapter.OAuthScopeKey);
                Assert.AreEqual(AuthenticationConstants.ToChannelFromBotOAuthScope, scope);
            });

            var sut = new BotFrameworkAdapter(credentialProvider);
            await sut.ProcessActivityAsync(
                identity,
                new Activity("test")
                {
                    ChannelId = Channels.Emulator,
                    ServiceUrl = serviceUrl
                },
                callback,
                CancellationToken.None);
        }

        [TestMethod]
        public async Task ContinueConversationAsyncWithoutAudience()
        {
            // Arrange
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var adapter = new BotFrameworkAdapter(mockCredentialProvider.Object, customHttpClient: httpClient);

            // Create ClaimsIdentity that represents Skill2-to-Skill1 communication
            var skill2AppId = "00skill2-aaaa-aaaa-aaaa-1a7454675036";
            var skill1AppId = "00skill1-bbbb-bbbb-bbbb-c8f7cfd51093";

            var skillClaims = new List<Claim>
            {
                new Claim(AuthenticationConstants.AudienceClaim, skill1AppId),
                new Claim(AuthenticationConstants.AppIdClaim, skill2AppId),
                new Claim(AuthenticationConstants.VersionClaim, "1.0")
            };
            var skillsIdentity = new ClaimsIdentity(skillClaims);
            var channelServiceUrl = "https://smba.trafficmanager.net";

            // Skill1 is calling ContinueSkillConversationAsync() to proactively send an Activity to Skill 2
            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                var adapter = turnContext.Adapter as BotFrameworkAdapter;
                var claimsIdentity = turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey);

                var credsCacheField = typeof(BotFrameworkAdapter).GetField("_appCredentialMap", BindingFlags.NonPublic | BindingFlags.Instance);
                var credsCache = (ConcurrentDictionary<string, AppCredentials>)credsCacheField.GetValue(adapter);

                Assert.AreEqual(1, credsCache.Count);

                var serviceUri = new Uri(channelServiceUrl);
                var clientCacheField = typeof(BotFrameworkAdapter).GetField("_connectorClients", BindingFlags.NonPublic | BindingFlags.Instance);
                var clientCache = (ConcurrentDictionary<string, ConnectorClient>)clientCacheField.GetValue(adapter);
                Assert.AreEqual(1, clientCache.Count);

                // Get AppCredentials for "skill1-to-channel" communications
                AppCredentials appCreds;
                credsCache.TryGetValue($"{skill1AppId}{AuthenticationConstants.ToChannelFromBotOAuthScope}", out appCreds);
                Assert.AreEqual(skill1AppId, appCreds.MicrosoftAppId);
                Assert.AreEqual(AuthenticationConstants.ToChannelFromBotOAuthScope, appCreds.OAuthScope);

                // Get "skill1-to-channel" ConnectorClient
                ConnectorClient toSkill2Client;
                clientCache.TryGetValue($"{channelServiceUrl}{skill1AppId}", out toSkill2Client);
                Assert.AreEqual(serviceUri, toSkill2Client.BaseUri);

                var turnStateClient = turnContext.TurnState.Get<IConnectorClient>();
                var clientCreds = turnStateClient.Credentials as AppCredentials;
                Assert.AreEqual(skill1AppId, clientCreds.MicrosoftAppId);
                Assert.AreEqual(AuthenticationConstants.ToChannelFromBotOAuthScope, clientCreds.OAuthScope);
            });

            // Create ConversationReference to send a proactive message from Skill1 to a channel
            var refs = new ConversationReference(serviceUrl: channelServiceUrl);

            await adapter.ContinueConversationAsync(skillsIdentity, refs, callback, default);
        }

        [TestMethod]
        public async Task ContinueConversationAsyncWithAudience()
        {
            // Arrange
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var adapter = new BotFrameworkAdapter(mockCredentialProvider.Object, customHttpClient: httpClient);

            // Create ClaimsIdentity that represents Skill2-to-Skill1 communication
            var skill2AppId = "00skill2-aaaa-aaaa-aaaa-1a7454675036";
            var skill1AppId = "00skill1-bbbb-bbbb-bbbb-c8f7cfd51093";

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

            await adapter.ContinueConversationAsync(skillsIdentity, refs, skill2AppId, callback, default);
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

        private static void GetCredsAndAssertValues(ITurnContext turnContext, string expectedAppId, string expectedScope, int? credsCount = null)
        {
            var credsCache = GetCache<ConcurrentDictionary<string, AppCredentials>>((BotFrameworkAdapter)turnContext.Adapter, CredsCacheName);
            credsCache.TryGetValue($"{expectedAppId}{expectedScope}", out var creds);
            AssertCredentialsValues(creds, expectedAppId, expectedScope);

            if (credsCount != null)
            {
                Assert.AreEqual(credsCount, credsCache.Count);
            }
        }

        private static void GetClientAndAssertValues(ITurnContext turnContext, string expectedAppId, string expectedScope, Uri expectedUrl, int? clientCount = null)
        {
            var clientCache = GetCache<ConcurrentDictionary<string, ConnectorClient>>((BotFrameworkAdapter)turnContext.Adapter, ClientsCacheName);
            clientCache.TryGetValue($"{expectedUrl}{expectedAppId}", out var client);
            AssertConnectorClientValues(client, expectedAppId, expectedUrl, expectedScope);

            if (clientCount != null)
            {
                Assert.AreEqual(clientCount, clientCache.Count);
            }
        }

        private static T GetCache<T>(BotFrameworkAdapter adapter, string fieldName)
        {
            var cacheField = typeof(BotFrameworkAdapter).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)cacheField.GetValue(adapter);
        }

        private static void AssertCredentialsValues(AppCredentials creds, string expectedAppId, string expectedScope = AuthenticationConstants.ToChannelFromBotOAuthScope)
        {
            Assert.AreEqual(expectedAppId, creds.MicrosoftAppId);
            Assert.AreEqual(expectedScope, creds.OAuthScope);
        }

        private static void AssertConnectorClientValues(IConnectorClient client, string expectedAppId, Uri expectedServiceUrl, string expectedScope = AuthenticationConstants.ToChannelFromBotOAuthScope)
        {
            var creds = (AppCredentials)client.Credentials;
            Assert.AreEqual(expectedAppId, creds.MicrosoftAppId);
            Assert.AreEqual(expectedScope, creds.OAuthScope);
            Assert.AreEqual(expectedServiceUrl, client.BaseUri);
        }
    }
}
