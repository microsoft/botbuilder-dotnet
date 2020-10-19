// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Teams.Tests
{
    public class TeamsInfoTests
    {
        [Fact]
        public async Task TestSendMessageToTeamsChannelAsync()
        {
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler(), false);

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("https://test.coffee"), new MicrosoftAppCredentials("big-guid-here", "appPasswordHere"), customHttpClient);
            
            var activity = new Activity
            {
                Type = "message",
                Text = "Test-SendMessageToTeamsChannelAsync",
                ChannelId = Channels.Msteams,
                ChannelData = new TeamsChannelData
                {
                    Team = new TeamInfo
                    {
                        Id = "team-id",
                    },
                },
            };
            
            var turnContext = new TurnContext(new BotFrameworkAdapter(new SimpleCredentialProvider("big-guid-here", "appPasswordHere"), customHttpClient: customHttpClient), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);
            turnContext.Activity.ServiceUrl = "https://test.coffee";
            var handler = new TestTeamsActivityHandler();
            await handler.OnTurnAsync(turnContext);
        }

        [Fact]
        public async Task TestGetTeamDetailsAsync()
        {
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = "message",
                Text = "Test-GetTeamDetailsAsync",
                ChannelId = Channels.Msteams,
                ChannelData = new TeamsChannelData
                {
                    Team = new TeamInfo
                    {
                        Id = "team-id",
                    },
                },
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);

            var handler = new TestTeamsActivityHandler();
            await handler.OnTurnAsync(turnContext);
        }

        [Fact]
        public async Task TestTeamGetMembersAsync()
        {
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = "message",
                Text = "Test-Team-GetMembersAsync",
                ChannelId = Channels.Msteams,
                ChannelData = new TeamsChannelData
                {
                    Team = new TeamInfo
                    {
                        Id = "team-id",
                    },
                },
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);

            var handler = new TestTeamsActivityHandler();
            await handler.OnTurnAsync(turnContext);
        }

        [Fact]
        public async Task TestGroupChatGetMembersAsync()
        {
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = "message",
                Text = "Test-GroupChat-GetMembersAsync",
                ChannelId = Channels.Msteams,
                Conversation = new ConversationAccount { Id = "conversation-id" },
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);

            var handler = new TestTeamsActivityHandler();
            await handler.OnTurnAsync(turnContext);
        }

        [Fact]
        public async Task TestGetChannelsAsync()
        {
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = "message",
                Text = "Test-GetChannelsAsync",
                ChannelId = Channels.Msteams,
                ChannelData = new TeamsChannelData
                {
                    Team = new TeamInfo
                    {
                        Id = "team-id",
                    },
                },
                ServiceUrl = "https://test.coffee",
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);
            var handler = new TestTeamsActivityHandler();
            await handler.OnTurnAsync(turnContext);
        }

        [Fact]
        public async Task TestGetParticipantAsync()
        {
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = "message",
                Text = "Test-GetParticipantAsync",
                ChannelId = Channels.Msteams,
                From = new ChannelAccount { AadObjectId = "participantId-1" },
                ChannelData = new TeamsChannelData
                {
                    Meeting = new TeamsMeetingInfo
                    {
                        Id = "meetingId-1"
                    },
                    Tenant = new TenantInfo
                    {
                        Id = "tenantId-1"
                    },
                },
                ServiceUrl = "https://test.coffee",
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);
            var handler = new TestTeamsActivityHandler();
            await handler.OnTurnAsync(turnContext);
        }

        [Fact]
        public async Task TestGetMemberAsync()
        {
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = "message",
                Text = "Test-GetGetMemberAsync",
                ChannelId = Channels.Msteams,
                ChannelData = new TeamsChannelData
                {
                    Team = new TeamInfo
                    {
                        Id = "team-id",
                    },
                },
                ServiceUrl = "https://test.coffee",
                From = new ChannelAccount() { Id = "id-1" }
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);
            var handler = new TestTeamsActivityHandler();
            await handler.OnTurnAsync(turnContext);
        }

        [Fact]
        public async Task TestGetMemberNoTeamAsync()
        {
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = "message",
                Text = "Test-GetGetMemberAsync",
                ChannelId = Channels.Msteams,
                ServiceUrl = "https://test.coffee",
                From = new ChannelAccount() { Id = "id-1" },
                Conversation = new ConversationAccount() { Id = "conversation-id" },
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);
            var handler = new TestTeamsActivityHandler();
            await handler.OnTurnAsync(turnContext);
        }

        private class TestTeamsActivityHandler : TeamsActivityHandler
        {
            public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
            {
                await base.OnTurnAsync(turnContext, cancellationToken);

                switch (turnContext.Activity.Text)
                {
                    case "Test-GetTeamDetailsAsync":
                        await CallGetTeamDetailsAsync(turnContext);
                        break;
                    case "Test-Team-GetMembersAsync":
                        await CallTeamGetMembersAsync(turnContext);
                        break;
                    case "Test-GroupChat-GetMembersAsync":
                        await CallGroupChatGetMembersAsync(turnContext);
                        break;
                    case "Test-GetChannelsAsync":
                        await CallGetChannelsAsync(turnContext);
                        break;
                    case "Test-SendMessageToTeamsChannelAsync":
                        await CallSendMessageToTeamsChannelAsync(turnContext);
                        break;
                    case "Test-GetGetMemberAsync":
                        await CallTeamGetMemberAsync(turnContext);
                        break;
                    case "Test-GetParticipantAsync":
                        await CallTeamsInfoGetParticipantAsync(turnContext);
                        break;
                    default:
                        Assert.True(false);
                        break;
                }
            }

            private async Task CallSendMessageToTeamsChannelAsync(ITurnContext turnContext)
            {
                var message = MessageFactory.Text("hi");
                var channelId = "channelId123";
                var creds = new MicrosoftAppCredentials("big-guid-here", "appPasswordHere");
                var cancelToken = new CancellationToken();
                var reference = await TeamsInfo.SendMessageToTeamsChannelAsync(turnContext, message, channelId, creds, cancelToken);

                Assert.Equal("activityId123", reference.Item1.ActivityId);
                Assert.Equal(channelId, reference.Item1.ChannelId);
                Assert.Equal(turnContext.Activity.ServiceUrl, reference.Item1.ServiceUrl);
                Assert.Equal("activityId123", reference.Item2);
            }

            private async Task CallGetTeamDetailsAsync(ITurnContext turnContext)
            {
                var teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext);

                Assert.Equal("team-id", teamDetails.Id);
                Assert.Equal("team-name", teamDetails.Name);
                Assert.Equal("team-aadgroupid", teamDetails.AadGroupId);
            }

            private async Task CallTeamGetMembersAsync(ITurnContext turnContext)
            {
                var members = (await TeamsInfo.GetMembersAsync(turnContext)).ToArray();

                Assert.Equal("id-1", members[0].Id);
                Assert.Equal("name-1", members[0].Name);
                Assert.Equal("givenName-1", members[0].GivenName);
                Assert.Equal("surname-1", members[0].Surname);
                Assert.Equal("userPrincipalName-1", members[0].UserPrincipalName);

                Assert.Equal("id-2", members[1].Id);
                Assert.Equal("name-2", members[1].Name);
                Assert.Equal("givenName-2", members[1].GivenName);
                Assert.Equal("surname-2", members[1].Surname);
                Assert.Equal("userPrincipalName-2", members[1].UserPrincipalName);
            }

            private async Task CallTeamGetMemberAsync(ITurnContext turnContext)
            {
                var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id);

                Assert.Equal("id-1", member.Id);
                Assert.Equal("name-1", member.Name);
                Assert.Equal("givenName-1", member.GivenName);
                Assert.Equal("surname-1", member.Surname);
                Assert.Equal("userPrincipalName-1", member.UserPrincipalName);
            }

            private async Task CallTeamsInfoGetParticipantAsync(ITurnContext turnContext)
            {
                var participant = await TeamsInfo.GetMeetingParticipantAsync(turnContext);

                Assert.Equal("Organizer", participant.MeetingRole);
                Assert.Equal("meetigConversationId-1", participant.Conversation.Id);
                Assert.Equal("userPrincipalName-1", participant.UserPrincipalName);
            }

            private async Task CallGroupChatGetMembersAsync(ITurnContext turnContext)
            {
                var members = (await TeamsInfo.GetMembersAsync(turnContext)).ToArray();

                Assert.Equal("id-3", members[0].Id);
                Assert.Equal("name-3", members[0].Name);
                Assert.Equal("givenName-3", members[0].GivenName);
                Assert.Equal("surname-3", members[0].Surname);
                Assert.Equal("userPrincipalName-3", members[0].UserPrincipalName);

                Assert.Equal("id-4", members[1].Id);
                Assert.Equal("name-4", members[1].Name);
                Assert.Equal("givenName-4", members[1].GivenName);
                Assert.Equal("surname-4", members[1].Surname);
                Assert.Equal("userPrincipalName-4", members[1].UserPrincipalName);
            }

            private async Task CallGetChannelsAsync(ITurnContext turnContext)
            {
                var channels = (await TeamsInfo.GetTeamChannelsAsync(turnContext)).ToArray();

                Assert.Equal("channel-id-1", channels[0].Id);

                Assert.Equal("channel-id-2", channels[1].Id);
                Assert.Equal("channel-name-2", channels[1].Name);

                Assert.Equal("channel-id-3", channels[2].Id);
                Assert.Equal("channel-name-3", channels[2].Name);
            }
        }

        private class RosterHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);

                // GetTeamDetails
                if (request.RequestUri.PathAndQuery.EndsWith("team-id"))
                {
                    var content = new JObject
                    {
                        new JProperty("id", "team-id"),
                        new JProperty("name", "team-name"),
                        new JProperty("aadGroupId", "team-aadgroupid"),
                    };
                    response.Content = new StringContent(content.ToString());
                }

                // SendMessageToThreadInTeams
                else if (request.RequestUri.PathAndQuery.EndsWith("v3/conversations"))
                {
                    var content = new JObject 
                    { 
                        new JProperty("id", "id123"),
                        new JProperty("serviceUrl", "https://serviceUrl/"),
                        new JProperty("activityId", "activityId123")
                    };
                    response.Content = new StringContent(content.ToString());
                }

                // GetChannels
                else if (request.RequestUri.PathAndQuery.EndsWith("team-id/conversations"))
                {
                    // Returns ConversationList 
                    var content = new JObject
                    {
                        new JProperty(
                            "conversations",
                            new JArray
                            {
                                new JObject { new JProperty("id", "channel-id-1") },
                                new JObject { new JProperty("id", "channel-id-2"), new JProperty("name", "channel-name-2") },
                                new JObject { new JProperty("id", "channel-id-3"), new JProperty("name", "channel-name-3") },
                            })
                    };
                    response.Content = new StringContent(content.ToString());
                }

                // GetMembers (Team)
                else if (request.RequestUri.PathAndQuery.EndsWith("team-id/members"))
                {
                    var content = new JArray
                    {
                        new JObject
                        {
                            new JProperty("id", "id-1"),
                            new JProperty("objectId", "objectId-1"),
                            new JProperty("name", "name-1"),
                            new JProperty("givenName", "givenName-1"),
                            new JProperty("surname", "surname-1"),
                            new JProperty("email", "email-1"),
                            new JProperty("userPrincipalName", "userPrincipalName-1"),
                            new JProperty("tenantId", "tenantId-1"),
                        },
                        new JObject
                        {
                            new JProperty("id", "id-2"),
                            new JProperty("objectId", "objectId-2"),
                            new JProperty("name", "name-2"),
                            new JProperty("givenName", "givenName-2"),
                            new JProperty("surname", "surname-2"),
                            new JProperty("email", "email-2"),
                            new JProperty("userPrincipalName", "userPrincipalName-2"),
                            new JProperty("tenantId", "tenantId-2"),
                        },
                    };
                    response.Content = new StringContent(content.ToString());
                }

                // GetMembers (Group Chat)
                else if (request.RequestUri.PathAndQuery.EndsWith("conversation-id/members"))
                {
                    var content = new JArray
                    {
                        new JObject
                        {
                            new JProperty("id", "id-3"),
                            new JProperty("objectId", "objectId-3"),
                            new JProperty("name", "name-3"),
                            new JProperty("givenName", "givenName-3"),
                            new JProperty("surname", "surname-3"),
                            new JProperty("email", "email-3"),
                            new JProperty("userPrincipalName", "userPrincipalName-3"),
                            new JProperty("tenantId", "tenantId-3"),
                        },
                        new JObject
                        {
                            new JProperty("id", "id-4"),
                            new JProperty("objectId", "objectId-4"),
                            new JProperty("name", "name-4"),
                            new JProperty("givenName", "givenName-4"),
                            new JProperty("surname", "surname-4"),
                            new JProperty("email", "email-4"),
                            new JProperty("userPrincipalName", "userPrincipalName-4"),
                            new JProperty("tenantId", "tenantId-4"),
                        },
                    };
                    response.Content = new StringContent(content.ToString());
                }

                // Get Member
                else if (request.RequestUri.PathAndQuery.EndsWith("team-id/members/id-1") || request.RequestUri.PathAndQuery.EndsWith("conversation-id/members/id-1"))
                {
                    var content = new JObject
                        {
                            new JProperty("id", "id-1"),
                            new JProperty("objectId", "objectId-1"),
                            new JProperty("name", "name-1"),
                            new JProperty("givenName", "givenName-1"),
                            new JProperty("surname", "surname-1"),
                            new JProperty("email", "email-1"),
                            new JProperty("userPrincipalName", "userPrincipalName-1"),
                            new JProperty("tenantId", "tenantId-1"),
                        };
                    response.Content = new StringContent(content.ToString());
                }

                // Get participant
                else if (request.RequestUri.PathAndQuery.EndsWith("v1/meetings/meetingId-1/participants/participantId-1?tenantId=tenantId-1"))
                {
                    var content = new JObject
                        {
                            new JProperty("meetingRole", "Organizer"),
                            new JProperty("userPrincipalName", "userPrincipalName-1"),
                            new JProperty("conversation", new JObject(new JProperty("Id", "meetigConversationId-1"))),
                        };
                    response.Content = new StringContent(content.ToString());
                }

                return Task.FromResult(response);
            }
        }
    }
}
