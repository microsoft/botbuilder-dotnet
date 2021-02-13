// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
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
        public async Task Action_GetMeetingParticipant()
        {
            var participantResult = GetParticipant().ToString();

            var uriToContent = new Dictionary<string, string>()
                {
                    { "/v1/meetings/meeting-id-1/participants/participant-aad-id-1?tenantId=tenant-id-1", participantResult },
                    { "/v1/meetings/customMeetingId/participants/customParticipantId?tenantId=customTenantId", participantResult }
                };

            var teamsMiddleware = GetTeamsMiddleware(uriToContent);
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, middleware: new[] { teamsMiddleware });
        }

        [Fact]
        public async Task Action_GetMeetingParticipantError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_GetMeetingParticipantErrorWithAdapter()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_GetMember()
        {
            var participantResult = GetParticipant().ToString();
            var uriToContent = new Dictionary<string, string>()
                {
                    { "/v3/conversations/Action_GetMember/members/member-id", participantResult },
                    { "/v3/conversations/Action_GetMember/members/customMemberId", participantResult }
                };

            var teamsMiddleware = GetTeamsMiddleware(uriToContent);
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, middleware: new[] { teamsMiddleware });
        }

        [Fact]
        public async Task Action_GetMemberError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_GetMemberErrorWithAdapter()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_GetPagedMembers()
        {
            var threeMembers = GenerateTeamMembers(3);
            threeMembers.ContinuationToken = "customToken";

            var twoMembers = GenerateTeamMembers(2);
            twoMembers.ContinuationToken = "token";

            var uriToContent = new Dictionary<string, string>()
                {
                    { "/v3/conversations/Action_GetPagedMembers/pagedmembers", JObject.FromObject(threeMembers).ToString() },
                    { "/v3/conversations/Action_GetPagedMembers/pagedmembers?pageSize=2&continuationToken=token", JObject.FromObject(twoMembers).ToString() }
                };

            var teamsMiddleware = GetTeamsMiddleware(uriToContent);
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, middleware: new[] { teamsMiddleware });
        }

        [Fact]
        public async Task Action_GetPagedMembersError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_GetPagedTeamMembers()
        {
            var threeMembers = GenerateTeamMembers(3);
            threeMembers.ContinuationToken = "token";

            var twoMembers = GenerateTeamMembers(2);
            twoMembers.ContinuationToken = "token";

            var uriToContent = new Dictionary<string, string>()
                {
                    { "/v3/conversations/team-id-1/pagedmembers", JObject.FromObject(threeMembers).ToString() },
                    { "/v3/conversations/team-id-1/pagedmembers?pageSize=2&continuationToken=token", JObject.FromObject(twoMembers).ToString() }
                };

            var teamsMiddleware = GetTeamsMiddleware(uriToContent);
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, middleware: new[] { teamsMiddleware });
        }

        [Fact]
        public async Task Action_GetPagedTeamMembersError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_GetTeamChannels()
        {
            var conversations = JObject.FromObject(new ConversationList
            {
                Conversations = new List<ChannelInfo>()
                    {
                        new ChannelInfo { Id = "19:ChannelIdgeneralChannelId@thread.skype", Name = "Testing0" },
                        new ChannelInfo { Id = "19:somechannelId2e5ab3df9ae9b594bdb@thread.skype", Name = "Testing1" },
                        new ChannelInfo { Id = "19:somechannelId388ade16aa4dd375e69@thread.skype", Name = "Testing2" },
                    }
            }).ToString();

            var uriToContent = new Dictionary<string, string>()
                {
                    { "/v3/teams/team-id-1/conversations", conversations },
                    { "/v3/teams/customTeamId/conversations", conversations }
                };

            var teamsMiddleware = GetTeamsMiddleware(uriToContent);
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, middleware: new[] { teamsMiddleware });
        }

        [Fact]
        public async Task Action_GetTeamChannelsError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_GetTeamDetails()
        {
            var details = JObject.FromObject(new TeamDetails
            {
                Id = "19:generalChannelIdgeneralChannelId@thread.skype",
                Name = "TeamName",
                AadGroupId = "Team-aadGroupId"
            }).ToString();

            var uriToContent = new Dictionary<string, string>()
                {
                    { "/v3/teams/team-id-1/team-id-1", details },
                    { "/v3/teams/customTeamId", details }
                };

            var teamsMiddleware = GetTeamsMiddleware(uriToContent);
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, middleware: new[] { teamsMiddleware });
        }

        [Fact]
        public async Task Action_GetTeamDetailsError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_GetTeamMember()
        {
            var member = JObject.FromObject(GenerateTeamMembers(1).Members.First()).ToString();

            var uriToContent = new Dictionary<string, string>()
                {
                    { "/v3/conversations/team-id-1/members/user1", member },
                    { "/v3/conversations/customTeamId/members/customMemberId", member }
                };

            var teamsMiddleware = GetTeamsMiddleware(uriToContent);
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, middleware: new[] { teamsMiddleware });
        }

        [Fact]
        public async Task Action_GetTeamMemberError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_GetTeamMemberErrorWithAdapter()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendAppBasedLinkQueryResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendAppBasedLinkQueryResponseError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        //[Fact]
        //public async Task Action_SendMessageToTeamsChannel()
        //{
        //    NOTE: Current test adapter is not a BotFrameworkAdapter,
        //           and does not support mocking SendMessageToTeamsChannel
        //    var teamsMiddleware = GetTeamsMiddleware(new JObject(), "/v3/conversations");
        //    await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, middleware: new[] { teamsMiddleware });
        //}
        
        [Fact]
        public async Task Action_SendMessageToTeamsChannelError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_SendMEActionResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendMEActionResponseError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendMEAttachmentsResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendMEAttachmentsResponseError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_SendMEAuthResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        //[Fact]
        //public async Task Action_SendMEAuthResponseError()
        //{
        //      NOTE: Current test adapter is not a BotFrameworkAdapter,
        //    await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        //}

        [Fact]
        public async Task Action_SendMEAuthResponseErrorWithAdapter()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_SendMEBotMessagePreviewResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendMEBotMessagePreviewResponseError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_SendMEConfigQuerySettingUrlResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendMEConfigQuerySettingUrlResponseError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_SendMEMessageResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendMEMessageResponseError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_SendMESelectItemResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendMESelectItemResponseError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_SendTaskModuleCardResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendTaskModuleCardResponseError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer, adapterChannel: Channels.Test);
        }

        [Fact]
        public async Task Action_SendTaskModuleMessageResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendTaskModuleUrlResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendTaskModuleUrlResponseError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task Action_SendTabCardResponseError()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }
        
        [Fact]
        public async Task Action_SendTabCardResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }
        
        [Fact]
        public async Task Action_SendTabAuthResponseErrorWithAdapter()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        //[Fact]
        //public async Task Action_SendTabAuthResponseError()
        //{
        //      NOTE: Current test adapter is not a BotFrameworkAdapter,
        //    await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        //}

        private IMiddleware GetErrorTeamsMiddleware(string exception)
        {
            // Create a connector client, setup with a custom httpclient which will 
            // throw an exception when the connectorclient's outgoing pipeline's SendAsync
            // is called
            var messageHandler = new ErrorHttpMessageHandler(exception);
            return GetTestConnectorClientMiddleware(messageHandler);
        }

        private ConversationReference GetGroupConversation()
        {
            return new ConversationReference
            {
                ChannelId = Channels.Msteams,
                User = new ChannelAccount
                {
                    Id = "29:User-Id",
                    Name = "User Name",
                    AadObjectId = "participant-aad-id"
                },
                Conversation = new ConversationAccount
                {
                    ConversationType = "groupChat",
                    TenantId = "tenantId-Guid",
                    Name = "group",
                    IsGroup = true,
                    Id = "19:groupChatId@thread.v2"
                }
            };
        }

        private TeamsPagedMembersResult GenerateTeamMembers(int total)
        {
            var accounts = new List<TeamsChannelAccount>();

            for (int count = 0; count < total; count++)
            {
                accounts.Add(new TeamsChannelAccount
                {
                    Id = $"29:User-Id-{count}",
                    Name = $"User Name-{count}",
                    AadObjectId = $"User-{count}-Object-Id",
                    Surname = $"Surname-{count}",
                    Email = $"User.{count}@microsoft.com",
                    UserPrincipalName = $"user{count}@microsoft.com",
                    TenantId = "tenant-id-1",
                    GivenName = "User"
                });
            }

            return new TeamsPagedMembersResult() { Members = accounts };
        }

        private JObject GetParticipant(bool groupConversation = false)
        {
            return JObject.FromObject(new
            {
                id = "29:User-Id-0",
                objectId = "User-0-Object-Id",
                name = "User Name-0",
                meeting = new { role = "Organizer" },
                surname = "Surname-0",
                tenantId = "tenant-id-1",
                userPrincipalName = "user0@microsoft.com",
                user = new
                {
                    userPrincipalName = "userPrincipalName-1",
                },
                email = "User.0@microsoft.com",
                givenName = "User",
                conversation = new
                {
                    id = groupConversation ? "19:groupChatId@thread.v2" : "a:oneOnOneConversationId",
                    name = groupConversation ? "group" : "oneOnOne",
                    tenantId = "tenantId-Guid",
                    conversationType = groupConversation ? "groupChat" : "personal",
                    isGroup = groupConversation,
                }
            });
        }

        private IMiddleware GetTeamsMiddleware(JObject result, string path = null)
        {
            // Create a connector client, setup with a custom httpclient which will return
            // the desired result through the TestHttpMessageHandler.
            TestsHttpMessageHandler messageHandler;
            if (!string.IsNullOrEmpty(path))
            {
                messageHandler = new TestsHttpMessageHandler(path, result.ToString());
            }
            else
            {
                messageHandler = new TestsHttpMessageHandler(result.ToString());
            }

            return GetTestConnectorClientMiddleware(messageHandler);
        }

        private IMiddleware GetTeamsMiddleware(Dictionary<string, string> results)
        {
            // Create a connector client, setup with a custom httpclient which will return
            // the desired result through the TestHttpMessageHandler.
            var messageHandler = new TestsHttpMessageHandler(results);
            return GetTestConnectorClientMiddleware(messageHandler);
        }

        private TestConnectorClientMiddleware GetTestConnectorClientMiddleware(HttpMessageHandler messageHandler)
        {
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

        private class ErrorHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _error;

            public ErrorHttpMessageHandler(string error)
            {
                _error = error;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new Exception(_error);
            }
        }

        // Message handler to mock returning a specific object when requested from a specified path.
        private class TestsHttpMessageHandler : HttpMessageHandler
        {
            private Dictionary<string, string> _uriToContent;
            private string _content;

            public TestsHttpMessageHandler(string url, string content)
                : this(new Dictionary<string, string>() { { url, content } })
            {
            }

            public TestsHttpMessageHandler(string content)
            {
                _content = content;
            }

            public TestsHttpMessageHandler(Dictionary<string, string> uriToContent)
            {
                _uriToContent = uriToContent;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                if (!string.IsNullOrEmpty(_content))
                {
                    response.Content = new StringContent(_content);
                }
                else
                {
                    var path = request.RequestUri.PathAndQuery;
                    foreach (var urlAndContent in _uriToContent)
                    {
                        if (urlAndContent.Key.Contains(path, System.StringComparison.OrdinalIgnoreCase))
                        {
                            response.Content = new StringContent(urlAndContent.Value);
                        }
                    }
                }

                return Task.FromResult(response);
            }
        }
    }
}
