// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.using System.Security.Claims;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class BotFrameworkAdapterTests
    {
        private const string AppCredentialsCacheName = "_appCredentialMap";
        private const string ConnectorClientsCacheName = "_connectorClients";

        [Fact]
        public async Task TenantIdShouldBeSetInConversationForTeams()
        {
            var activity = await ProcessActivity(Channels.Msteams, "theTenantId", null);
            Assert.Equal("theTenantId", activity.Conversation.TenantId);
        }

        [Fact]
        public async Task TenantIdShouldNotChangeInConversationForTeamsIfPresent()
        {
            var activity = await ProcessActivity(Channels.Msteams, "theTenantId", "shouldNotBeReplaced");
            Assert.Equal("shouldNotBeReplaced", activity.Conversation.TenantId);
        }

        [Fact]
        public async Task TenantIdShouldNotBeSetInConversationIfNotTeams()
        {
            var activity = await ProcessActivity(Channels.Directline, "theTenantId", null);
            Assert.Null(activity.Conversation.TenantId);
        }

        [Fact]
        public async Task TenantIdShouldNotFailIfNoChannelData()
        {
            var activity = await ProcessActivity(Channels.Directline, null, null);
            Assert.Null(activity.Conversation.TenantId);
        }

        [Fact]
        public async Task CreateConversationOverloadProperlySetsTenantId()
        {
            // Arrange
            const string activityIdName = "ActivityId";
            const string activityIdValue = "SendActivityId";
            const string conversationIdName = "Id";
            const string conversationIdValue = "NewConversationId";
            const string tenantIdValue = "theTenantId";
            const string eventActivityName = "CreateConversation";

            Task<HttpResponseMessage> CreateResponseMessage()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(new JObject
                    {
                        { activityIdName, activityIdValue },
                        { conversationIdName, conversationIdValue }
                    }.ToString())
                };
                return Task.FromResult(response);
            }

            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => CreateResponseMessage());

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
            Assert.Equal(tenantIdValue, JObject.FromObject(newActivity.ChannelData)["tenant"]["tenantId"]);
            Assert.Equal(activityIdValue, newActivity.Id);
            Assert.Equal(conversationIdValue, newActivity.Conversation.Id);
            Assert.Equal(tenantIdValue, newActivity.Conversation.TenantId);
            Assert.Equal(eventActivityName, newActivity.Name);
        }

        [Fact]
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

                await turnContext.SendActivityAsync(reply, default);
            }

            var sentActivity = mockConnector.MemoryConversations.SentActivities.FirstOrDefault(f => f.Type == ActivityTypes.Message);

            // Assert - assert the reply's id is not sent
            Assert.Null(sentActivity.Id); 
        }

        [Theory]
        [InlineData(null, null, null, AuthenticationConstants.ToChannelFromBotOAuthScope, 0, 1)]
        [InlineData("00000000-0000-0000-0000-000000000001", CallerIdConstants.PublicAzureChannel, null, AuthenticationConstants.ToChannelFromBotOAuthScope, 1, 1)]
        [InlineData("00000000-0000-0000-0000-000000000001", CallerIdConstants.USGovChannel, GovernmentAuthenticationConstants.ChannelService, GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope, 1, 1)]
        public async Task ProcessActivityAsyncCreatesCorrectCredsAndClient(string botAppId, string expectedCallerId, string channelService, string expectedScope, int expectedAppCredentialsCount, int expectedClientCredentialsCount)
        {
            var claims = new List<Claim>();
            if (botAppId != null)
            {
                claims.Add(new Claim(AuthenticationConstants.AudienceClaim, botAppId));
                claims.Add(new Claim(AuthenticationConstants.AppIdClaim, botAppId));
                claims.Add(new Claim(AuthenticationConstants.VersionClaim, "1.0"));
            }

            var identity = new ClaimsIdentity(claims);

            var credentialProvider = new SimpleCredentialProvider { AppId = botAppId };
            var serviceUrl = "https://smba.trafficmanager.net/amer/";
            var callback = new BotCallbackHandler((context, ct) =>
            {
                GetAppCredentialsAndAssertValues(context, botAppId, expectedScope, expectedAppCredentialsCount);
                GetConnectorClientsAndAssertValues(
                    context,
                    botAppId,
                    expectedScope,
                    new Uri(serviceUrl),
                    expectedClientCredentialsCount);

                var scope = context.TurnState.Get<string>(BotAdapter.OAuthScopeKey);
                Assert.Equal(expectedCallerId, context.Activity.CallerId);
                return Task.CompletedTask;
            });

            var sut = new BotFrameworkAdapter(credentialProvider, new SimpleChannelProvider(channelService));
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

        [Fact]
        public async Task ProcessActivityAsyncForForwardedActivity()
        {
            var botAppId = "00000000-0000-0000-0000-000000000001";
            var skill1AppId = "00000000-0000-0000-0000-000000skill1";
            var claims = new List<Claim>
            {
                new Claim(AuthenticationConstants.AudienceClaim, skill1AppId),
                new Claim(AuthenticationConstants.AppIdClaim, botAppId),
                new Claim(AuthenticationConstants.VersionClaim, "1.0")
            };
            var identity = new ClaimsIdentity(claims);

            var credentialProvider = new SimpleCredentialProvider() { AppId = botAppId };
            var serviceUrl = "https://root-bot.test.azurewebsites.net/";
            var callback = new BotCallbackHandler((context, ct) =>
            {
                GetAppCredentialsAndAssertValues(context, skill1AppId, botAppId, 1);
                GetConnectorClientsAndAssertValues(
                    context,
                    skill1AppId,
                    botAppId,
                    new Uri(serviceUrl),
                    1);

                var scope = context.TurnState.Get<string>(BotAdapter.OAuthScopeKey);
                Assert.Equal(botAppId, scope);
                Assert.Equal($"{CallerIdConstants.BotToBotPrefix}{botAppId}", context.Activity.CallerId);
                return Task.CompletedTask;
            });

            var sut = new BotFrameworkAdapter(credentialProvider);
            await sut.ProcessActivityAsync(
                identity,
                new Activity("From root-bot")
                {
                    ChannelId = Channels.Emulator,
                    ServiceUrl = serviceUrl
                },
                callback,
                CancellationToken.None);
        }

        [Fact]
        public async Task ContinueConversationAsyncWithoutAudience()
        {
            // Arrange
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var adapter = new BotFrameworkAdapter(mockCredentialProvider.Object, customHttpClient: httpClient);

            // Create ClaimsIdentity that represents Skill2-to-Skill1 communication
            var skill2AppId = "00000000-0000-0000-0000-000000skill2";
            var skill1AppId = "00000000-0000-0000-0000-000000skill1";

            var skillClaims = new List<Claim>
            {
                new Claim(AuthenticationConstants.AudienceClaim, skill1AppId),
                new Claim(AuthenticationConstants.AppIdClaim, skill2AppId),
                new Claim(AuthenticationConstants.VersionClaim, "1.0")
            };
            var skillsIdentity = new ClaimsIdentity(skillClaims);
            var channelServiceUrl = "https://continuetest.smba.trafficmanager.net/amer/";

            // Skill1 is calling ContinueSkillConversationAsync() to proactively send an Activity to the channel
            var callback = new BotCallbackHandler((turnContext, ct) =>
            {
                GetAppCredentialsAndAssertValues(turnContext, skill1AppId, AuthenticationConstants.ToChannelFromBotOAuthScope, 1);
                GetConnectorClientsAndAssertValues(
                    turnContext,
                    skill1AppId,
                    AuthenticationConstants.ToChannelFromBotOAuthScope,
                    new Uri(channelServiceUrl),
                    1);

                // Get "skill1-to-channel" ConnectorClient off of TurnState
                var contextAdapter = turnContext.Adapter as BotFrameworkAdapter;
                var clientCache = GetCache<ConcurrentDictionary<string, ConnectorClient>>(contextAdapter, ConnectorClientsCacheName);
                clientCache.TryGetValue($"{channelServiceUrl}{skill1AppId}:{AuthenticationConstants.ToChannelFromBotOAuthScope}", out var client);

                var turnStateClient = turnContext.TurnState.Get<IConnectorClient>();
                var clientCreds = turnStateClient.Credentials as AppCredentials;

                Assert.Equal(skill1AppId, clientCreds.MicrosoftAppId);
                Assert.Equal(AuthenticationConstants.ToChannelFromBotOAuthScope, clientCreds.OAuthScope);
                Assert.Equal(client.BaseUri, turnStateClient.BaseUri);

                var scope = turnContext.TurnState.Get<string>(BotAdapter.OAuthScopeKey);
                Assert.Equal(AuthenticationConstants.ToChannelFromBotOAuthScope, scope);

                // Ensure the serviceUrl was added to the trusted hosts
                Assert.True(AppCredentials.TrustedHostNames.ContainsKey(new Uri(channelServiceUrl).Host));

                return Task.CompletedTask;
            });

            // Create ConversationReference to send a proactive message from Skill1 to a channel
            var refs = new ConversationReference(serviceUrl: channelServiceUrl);

            // Ensure the serviceUrl is NOT in the trusted hosts
            Assert.False(AppCredentials.TrustedHostNames.ContainsKey(new Uri(channelServiceUrl).Host));

            await adapter.ContinueConversationAsync(skillsIdentity, refs, callback, default);
        }

        [Fact]
        public async Task ContinueConversationAsyncWithAudience()
        {
            // Arrange
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var adapter = new BotFrameworkAdapter(mockCredentialProvider.Object, customHttpClient: httpClient);

            // Create ClaimsIdentity that represents Skill2-to-Skill1 communication
            var skill2AppId = "00000000-0000-0000-0000-000000skill2";
            var skill1AppId = "00000000-0000-0000-0000-000000skill1";

            var skillClaims = new List<Claim>
            {
                new Claim(AuthenticationConstants.AudienceClaim, skill1AppId),
                new Claim(AuthenticationConstants.AppIdClaim, skill2AppId),
                new Claim(AuthenticationConstants.VersionClaim, "1.0")
            };
            var skillsIdentity = new ClaimsIdentity(skillClaims);
            var skill2ServiceUrl = "https://continuetest.skill2.com/api/skills/";

            // Skill1 is calling ContinueSkillConversationAsync() to proactively send an Activity to Skill 2
            var callback = new BotCallbackHandler((turnContext, ct) =>
            {
                GetAppCredentialsAndAssertValues(turnContext, skill1AppId, skill2AppId, 1);
                GetConnectorClientsAndAssertValues(
                    turnContext,
                    skill1AppId,
                    skill2AppId,
                    new Uri(skill2ServiceUrl),
                    1);

                // Get "skill1-to-skill2" ConnectorClient off of TurnState
                var contextAdapter = turnContext.Adapter as BotFrameworkAdapter;
                var clientCache = GetCache<ConcurrentDictionary<string, ConnectorClient>>(contextAdapter, ConnectorClientsCacheName);
                clientCache.TryGetValue($"{skill2ServiceUrl}{skill1AppId}:{skill2AppId}", out var client);

                var turnStateClient = turnContext.TurnState.Get<IConnectorClient>();
                var clientCreds = turnStateClient.Credentials as AppCredentials;

                Assert.Equal(skill1AppId, clientCreds.MicrosoftAppId);
                Assert.Equal(skill2AppId, clientCreds.OAuthScope);
                Assert.Equal(client.BaseUri, turnStateClient.BaseUri);

                var scope = turnContext.TurnState.Get<string>(BotAdapter.OAuthScopeKey);
                Assert.Equal(skill2AppId, scope);

                // Ensure the serviceUrl was added to the trusted hosts
                Assert.True(AppCredentials.TrustedHostNames.ContainsKey(new Uri(skill2ServiceUrl).Host));

                return Task.CompletedTask;
            });

            // Create ConversationReference to send a proactive message from Skill1 to Skill2
            var refs = new ConversationReference(serviceUrl: skill2ServiceUrl);

            // Ensure the serviceUrl is NOT in the trusted hosts
            Assert.False(AppCredentials.TrustedHostNames.ContainsKey(new Uri(skill2ServiceUrl).Host));

            await adapter.ContinueConversationAsync(skillsIdentity, refs, skill2AppId, callback, default);
        }

        [Fact]
        public async Task ProcessContinueConversationEvent()
        {
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var adapter = new BotFrameworkAdapter(mockCredentialProvider.Object, customHttpClient: httpClient);

            var cr = new ConversationReference
            {
                ActivityId = "activityId",
                Bot = new ChannelAccount
                {
                    Id = "channelId",
                    Name = "testChannelAccount",
                    Role = "bot",
                },
                ChannelId = "testChannel",
                ServiceUrl = "https://fake.service.url",
                Conversation = new ConversationAccount
                {
                    ConversationType = string.Empty,
                    Id = "testConversationId",
                    IsGroup = false,
                    Name = "testConversationName",
                    Role = "user",
                },
                User = new ChannelAccount
                {
                    Id = "channelId",
                    Name = "testChannelAccount",
                    Role = "bot",
                },
            };

            var activity = cr.GetContinuationActivity();
            activity.Value = "test";

            // Create ClaimsIdentity that represents Skill1-to-Skill1 communication
            var appId = "00000000-0000-0000-0000-000000skill1";

            var claims = new List<Claim>
            {
                new Claim(AuthenticationConstants.AudienceClaim, appId),
                new Claim(AuthenticationConstants.AppIdClaim, appId),
                new Claim(AuthenticationConstants.VersionClaim, "1.0")
            };
            var identity = new ClaimsIdentity(claims);

            var callback = new BotCallbackHandler((turnContext, ct) =>
            {
                var cr2 = turnContext.Activity.GetConversationReference();
                cr.ActivityId = null; // activityIds will be different...
                cr2.ActivityId = null;
                Assert.Equal(JsonConvert.SerializeObject(cr), JsonConvert.SerializeObject(cr2));
                Assert.Equal("test", (string)turnContext.Activity.Value);

                return Task.CompletedTask;
            });

            await adapter.ProcessActivityAsync(identity, (Activity)activity, callback, default);
        }

        [Fact]
        public async Task DeliveryModeExpectReplies()
        {
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var adapter = new BotFrameworkAdapter(new SimpleCredentialProvider(), customHttpClient: httpClient);

            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("activity 1"), ct);
                await turnContext.SendActivityAsync(MessageFactory.Text("activity 2"), ct);
                await turnContext.SendActivityAsync(MessageFactory.Text("activity 3"), ct);
            });

            var inboundActivity = new Activity
            {
                Type = ActivityTypes.Message,
                ChannelId = Channels.Emulator,
                ServiceUrl = "http://tempuri.org/whatever",
                DeliveryMode = DeliveryModes.ExpectReplies,
                Text = "hello world"
            };

            var invokeResponse = await adapter.ProcessActivityAsync(string.Empty, inboundActivity, callback, CancellationToken.None);

            Assert.Equal((int)HttpStatusCode.OK, invokeResponse.Status);
            var activities = ((ExpectedReplies)invokeResponse.Body).Activities;
            Assert.Equal(3, activities.Count);
            Assert.Equal("activity 1", activities[0].Text);
            Assert.Equal("activity 2", activities[1].Text);
            Assert.Equal("activity 3", activities[2].Text);
            mockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task DeliveryModeNormal()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(CreateInternalHttpResponse()));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var adapter = new BotFrameworkAdapter(new SimpleCredentialProvider(), customHttpClient: httpClient);

            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("activity 1"), ct);
                await turnContext.SendActivityAsync(MessageFactory.Text("activity 2"), ct);
                await turnContext.SendActivityAsync(MessageFactory.Text("activity 3"), ct);
            });

            var inboundActivity = new Activity
            {
                Type = ActivityTypes.Message,
                ChannelId = Channels.Emulator,
                ServiceUrl = "http://tempuri.org/whatever",
                DeliveryMode = DeliveryModes.Normal,
                Text = "hello world",
                Conversation = new ConversationAccount { Id = "conversationId" }
            };

            var invokeResponse = await adapter.ProcessActivityAsync(string.Empty, inboundActivity, callback, CancellationToken.None);

            Assert.Null(invokeResponse);
            mockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Exactly(3), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        private static HttpResponseMessage CreateInternalHttpResponse()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(new JObject { { "id", "SendActivityId" } }.ToString())
            };
            return response;
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

        private static void GetAppCredentialsAndAssertValues(ITurnContext turnContext, string expectedAppId, string expectedScope, int credsCount)
        {
            if (credsCount > 0)
            {
                var credsCache = GetCache<ConcurrentDictionary<string, AppCredentials>>((BotFrameworkAdapter)turnContext.Adapter, AppCredentialsCacheName);
                var cacheKey = $"{expectedAppId}{expectedScope}";
                credsCache.TryGetValue(cacheKey, out var creds);
                Assert.Equal(credsCount, credsCache.Count);

                Assert.Equal(expectedAppId, creds.MicrosoftAppId);
                Assert.Equal(expectedScope, creds.OAuthScope);
            }
        }

        private static void GetConnectorClientsAndAssertValues(ITurnContext turnContext, string expectedAppId, string expectedScope, Uri expectedUrl, int clientCount)
        {
            var clientCache = GetCache<ConcurrentDictionary<string, ConnectorClient>>((BotFrameworkAdapter)turnContext.Adapter, ConnectorClientsCacheName);
            var cacheKey = expectedAppId == null ? $"{expectedUrl}:" : $"{expectedUrl}{expectedAppId}:{expectedScope}";
            clientCache.TryGetValue(cacheKey, out var client);

            Assert.Equal(clientCount, clientCache.Count);
            var creds = (AppCredentials)client?.Credentials;
            Assert.Equal(expectedAppId, creds?.MicrosoftAppId);
            Assert.Equal(expectedScope, creds?.OAuthScope);
            Assert.Equal(expectedUrl, client?.BaseUri);
        }

        private static T GetCache<T>(BotFrameworkAdapter adapter, string fieldName)
        {
            var cacheField = typeof(BotFrameworkAdapter).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)cacheField.GetValue(adapter);
        }
    }
}
