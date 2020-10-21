// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Tests
{
    [CollectionDefinition("Dialogs.Adaptive")]
    public class ActionTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public ActionTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new AdaptiveTestingComponentRegistration());
            ComponentRegistration.Add(new TeamsComponentRegistration());

            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(ActionTests));
        }

        [Fact]
        public async Task Action_GetMeetingParticipantError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_GetMeetingParticipantMockedResults()
        {
            var participantResult = new JObject
                        {
                            new JProperty("meeting", new JObject(new JProperty("role", "Organizer"))),
                            new JProperty("user", new JObject(new JProperty("userPrincipalName", "userPrincipalName-1"))),
                            new JProperty("conversation", new JObject(new JProperty("Id", "meetigConversationId-1"))),
                        };

            var teamsMiddleware = GetTeamsMiddleware("/v1/meetings/meeting-id-1/participants/participant-aad-id-1?tenantId=tenant-id-1", participantResult);
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, middleware: new[] { teamsMiddleware });
        }

        private IMiddleware GetTeamsMiddleware(string path, JObject result)
        {
            // Create a connector client, setup with a custom httpclient which will return
            // the desired result through the TestHttpMessageHandler.
            var messageHandler = new TestsHttpMessageHandler(path, result.ToString());
            var testHttpClient = new HttpClient(messageHandler);
            testHttpClient.BaseAddress = new Uri("https://localhost.coffee");
            var testConnectorClient = new ConnectorClient(new Uri("http://localhost.coffee/"), new MicrosoftAppCredentials(string.Empty, string.Empty), testHttpClient);
            return new TestConnectorClientMiddleware(testConnectorClient);
        }

        // This middleware sets the turnstate's connector client, 
        // so it will be found by the adapter, and a new one not created.
        private class TestConnectorClientMiddleware : IMiddleware
        {
            private IConnectorClient _connectorClient;

            public TestConnectorClientMiddleware(IConnectorClient connectorClient)
            {
                _connectorClient = connectorClient;
            }

            public Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
            {
                turnContext.TurnState.Add<IConnectorClient>(_connectorClient);
                return next(cancellationToken);
            }
        }

        // Message handler to mock returning a specific object when requested from a specified path.
        private class TestsHttpMessageHandler : HttpMessageHandler
        {
            private Dictionary<string, string> _uriToContent;

            public TestsHttpMessageHandler(string url, string content)
                : this(new Dictionary<string, string>() { { url, content } })
            {
            }

            public TestsHttpMessageHandler(Dictionary<string, string> uriToContent)
            {
                _uriToContent = uriToContent;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);

                var path = request.RequestUri.PathAndQuery;
                foreach (var urlAndContent in _uriToContent)
                {
                    if (urlAndContent.Key.Contains(path, System.StringComparison.OrdinalIgnoreCase))
                    {
                        response.Content = new StringContent(urlAndContent.Value);
                    }
                }

                return Task.FromResult(response);
            }
        }
    }
}
