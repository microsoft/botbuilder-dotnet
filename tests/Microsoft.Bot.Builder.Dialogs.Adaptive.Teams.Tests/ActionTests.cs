// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
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
                            new JProperty("meetingRole", "Organizer"),
                            new JProperty("userPrincipalName", "userPrincipalName-1"),
                            new JProperty("conversation", new JObject(new JProperty("Id", "meetigConversationId-1"))),
                        };

            var messageHandler = new TestsHttpMessageHandler("/v1/meetings/meeting-id-1/participants/participant-aad-id-1?tenantId=tenant-id-1", participantResult.ToString());
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, testHttpClientMessageHandler: messageHandler);
        }

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
