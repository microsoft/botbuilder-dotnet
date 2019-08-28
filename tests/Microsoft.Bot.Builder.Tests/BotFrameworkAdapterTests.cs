// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.using System.Security.Claims;

using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.TransientFaultHandling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
            var mockClaims = new Mock<ClaimsIdentity>();
            var mockCredentialProvider = new Mock<ICredentialProvider>();

            var sut = new MockAdapter(mockCredentialProvider.Object);

            var activity = await ProcessActivity(Channels.Msteams, "theTenantId", null);
            var parameters = new ConversationParameters()
            {
                Activity = new Activity()
                {
                    ChannelData = activity.ChannelData,
                },
            };
            var reference = new ConversationReference()
            {
                ActivityId = activity.Id,
                Bot = activity.Recipient,
                ChannelId = activity.ChannelId,
                Conversation = activity.Conversation,
                ServiceUrl = activity.ServiceUrl,
                User = activity.From,
            };

            var credentials = new MicrosoftAppCredentials(string.Empty, string.Empty);

            Activity newActivity = null;

            Task UpdateParameters(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                newActivity = turnContext.Activity;
                return Task.CompletedTask;
            }

            await sut.CreateConversationAsync(activity.ChannelId, activity.ServiceUrl, credentials, parameters, UpdateParameters, reference, new CancellationToken());
            Assert.AreEqual("theTenantId", newActivity.ChannelData.GetType().GetProperty("TenantId").GetValue(newActivity.ChannelData, null));
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

        private class MockAdapter : BotFrameworkAdapter
        {
            private const string BotIdentityKey = "BotIdentity";

            public MockAdapter(
                ICredentialProvider credentialProvider,
                IChannelProvider channelProvider = null,
                RetryPolicy connectorClientRetryPolicy = null,
                HttpClient customHttpClient = null,
                IMiddleware middleware = null,
                ILogger logger = null)
                : base(credentialProvider, channelProvider, connectorClientRetryPolicy, customHttpClient, middleware, logger)
            {
            }

            public async override Task CreateConversationAsync(string channelId, string serviceUrl, MicrosoftAppCredentials credentials, ConversationParameters conversationParameters, BotCallbackHandler callback, CancellationToken cancellationToken)
            {
                var activity = conversationParameters.Activity;
                activity.ChannelData = new
                {
                    conversationParameters.TenantId,
                };
                await RunPipelineAsync(new TurnContext(this, conversationParameters.Activity), callback, cancellationToken).ConfigureAwait(false);

                return;
            }
        }
    }
}
