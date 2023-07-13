// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
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
using Microsoft.Rest;
using Moq;
using Newtonsoft.Json;
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

            var connectorClient = new ConnectorClient(new Uri("https://test.coffee"), MicrosoftAppCredentials.Empty, customHttpClient);

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

            var turnContext = new TurnContext(new BotFrameworkAdapter(new SimpleCredentialProvider(), customHttpClient: customHttpClient), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);
            turnContext.Activity.ServiceUrl = "https://test.coffee";
            var handler = new TestTeamsActivityHandler();
            await handler.OnTurnAsync(turnContext);
        }

        [Fact]
        public async Task TestSendMessageToTeamsChannel2Async()
        {
            // Arrange

            var expectedTeamsChannelId = "teams-channel-id";
            var expectedAppId = "app-id";
            var expectedServiceUrl = "service-url";
            var expectedActivityId = "activity-id";
            var expectedConversationId = "conversation-id";

            var requestActivity = new Activity { ServiceUrl = expectedServiceUrl };

            var adapter = new TestCreateConversationAdapter(expectedActivityId, expectedConversationId);

            var turnContextMock = new Mock<ITurnContext>();
            turnContextMock.Setup(tc => tc.Activity).Returns(requestActivity);
            turnContextMock.Setup(tc => tc.Adapter).Returns(adapter);

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

            // Act

            var r = await TeamsInfo.SendMessageToTeamsChannelAsync(turnContextMock.Object, activity, expectedTeamsChannelId, expectedAppId, CancellationToken.None);

            // Assert

            Assert.Equal(expectedConversationId, r.Item1.Conversation.Id);
            Assert.Equal(expectedActivityId, r.Item2);
            Assert.Equal(expectedAppId, adapter.AppId);
            Assert.Equal(Channels.Msteams, adapter.ChannelId);
            Assert.Equal(expectedServiceUrl, adapter.ServiceUrl);
            Assert.Null(adapter.Audience);

            var channelData = adapter.ConversationParameters.ChannelData;

            var channel = channelData.GetType().GetProperty("Channel").GetValue(channelData, null);
            var id = channel.GetType().GetProperty("Id").GetValue(channel, null);

            Assert.Equal(expectedTeamsChannelId, id);
            Assert.Equal(adapter.ConversationParameters.Activity, activity);
        }

        [Fact]
        public async Task TestGetMeetingInfoAsync()
        {
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = "message",
                Text = "Test-GetMeetingInfoAsync",
                ChannelId = Channels.Msteams,
                ChannelData = new TeamsChannelData
                {
                    Meeting = new TeamsMeetingInfo
                    {
                        Id = "meeting-id"
                    }
                },
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);

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

        [Theory]
        [InlineData("202")]
        [InlineData("207")]
        [InlineData("400")]
        [InlineData("403")]
        public async Task TestSendMeetingNotificationAsync(string statusCode)
        {
            // 202: accepted
            // 207: if the notifications are sent only to parital number of recipients because 
            //             the validation on some recipients’ ids failed or some recipients were not found in the roster. 
            //         • In this case, SMBA will return the user MRIs of those failed recipients in a format that was given to a bot 
            //             (ex: if a bot sent encrypted user MRIs, return encrypted one).

            // 400: when Meeting Notification request payload validation fails. For instance, 
            //     • Recipients: # of recipients is greater than what the API allows || all of recipients’ user ids were invalid
            //     • Surface: 
            //         o Surface list is empty or null 
            //         o Surface type is invalid 
            //         o Duplicative surface type exists in one payload
            // 403: if the bot is not allowed to send the notification.
            //     In this case, the payload should contain more detail error message.
            //     There can be many reasons: bot disabled by tenant admin, blocked during live site mitigation,
            //     the bot does not have a correct RSC permission for a specific surface type, etc

            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = "targetedMeetingNotification",
                Text = "Test-SendMeetingNotificationAsync",
                ChannelId = Channels.Msteams,
                ServiceUrl = "https://test.coffee",
                From = new ChannelAccount()
                {
                    Id = "id-1",

                    // Hack for test. use the Name field to pass expected status code to test code
                    Name = statusCode
                },
                Conversation = new ConversationAccount() { Id = "conversation-id" }
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);
            var handler = new TestTeamsActivityHandler();
            await handler.OnTurnAsync(turnContext);
        }

        [Theory]
        [InlineData("201")]
        [InlineData("400")]
        [InlineData("403")]
        [InlineData("429")]
        public async Task TestSendMessageToListOfUsersAsync(string statusCode)
        {
            // 201: created
            // 400: when send message to list of users request payload validation fails.
            // 403: if the bot is not allowed to send messages.
            // 429: too many requests for throttled requests.

            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = "message",
                Text = "Test-SendMessageToListOfUsersAsync",
                ChannelId = Channels.Msteams,
                ServiceUrl = "https://test.coffee",
                From = new ChannelAccount()
                {
                    Id = "id-1",

                    // Hack for test. use the Name field to pass expected status code to test code
                    Name = statusCode
                },
                Conversation = new ConversationAccount() { Id = "conversation-id" }
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);
            var handler = new TestTeamsActivityHandler();
            await handler.OnTurnAsync(turnContext);
        }

        [Theory]
        [InlineData("201")]
        [InlineData("400")]
        [InlineData("403")]
        [InlineData("429")]
        public async Task TestSendMessageToAllUsersInTenantAsync(string statusCode)
        {
            // 201: created
            // 400: when send message to list of users request payload validation fails.
            // 403: if the bot is not allowed to send messages.
            // 429: too many requests for throttled requests.

            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = "message",
                Text = "Test-SendMessageToAllUsersInTenantAsync",
                ChannelId = Channels.Msteams,
                ServiceUrl = "https://test.coffee",
                From = new ChannelAccount()
                {
                    Id = "id-1",

                    // Hack for test. use the Name field to pass expected status code to test code
                    Name = statusCode
                },
                Conversation = new ConversationAccount() { Id = "conversation-id" }
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);
            var handler = new TestTeamsActivityHandler();
            await handler.OnTurnAsync(turnContext);
        }

        [Theory]
        [InlineData("201")]
        [InlineData("400")]
        [InlineData("403")]
        [InlineData("404")]
        [InlineData("429")]
        public async Task TestSendMessageToAllUsersInTeamAsync(string statusCode)
        {
            // 201: created
            // 400: when send message to list of users request payload validation fails.
            // 403: if the bot is not allowed to send messages.
            // 404: when Team is not found.
            // 429: too many requests for throttled requests.

            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = "message",
                Text = "Test-SendMessageToAllUsersInTeamAsync",
                ChannelId = Channels.Msteams,
                ServiceUrl = "https://test.coffee",
                From = new ChannelAccount()
                {
                    Id = "id-1",

                    // Hack for test. use the Name field to pass expected status code to test code
                    Name = statusCode
                },
                Conversation = new ConversationAccount() { Id = "conversation-id" }
            };

            var turnContext = new TurnContext(new SimpleAdapter(), activity);
            turnContext.TurnState.Add<IConnectorClient>(connectorClient);
            var handler = new TestTeamsActivityHandler();
            await handler.OnTurnAsync(turnContext);
        }
        
        [Theory]
        [InlineData("201")]
        [InlineData("400")]
        [InlineData("403")]
        [InlineData("429")]
        public async Task TestSendMessageToListOfChannelsAsync(string statusCode)
        {
            // 201: created
            // 400: when send message to list of channels request payload validation fails.
            // 403: if the bot is not allowed to send messages.
            // 429: too many requests for throttled requests.

            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient(new RosterHttpMessageHandler());

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connectorClient = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            var activity = new Activity
            {
                Type = "message",
                Text = "Test-SendMessageToListOfChannelsAsync",
                ChannelId = Channels.Msteams,
                ServiceUrl = "https://test.coffee",
                From = new ChannelAccount()
                {
                    Id = "id-1",

                    // Hack for test. use the Name field to pass expected status code to test code
                    Name = statusCode
                },
                Conversation = new ConversationAccount() { Id = "conversation-id" }
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
                    case "Test-GetMeetingInfoAsync":
                        await CallTeamsInfoGetMeetingInfoAsync(turnContext);
                        break;
                    case "Test-SendMeetingNotificationAsync":
                        await CallSendMeetingNotificationAsync(turnContext);
                        break;
                    case "Test-SendMessageToListOfUsersAsync":
                        await CallSendMessageToListOfUsersAsync(turnContext);
                        break;
                    case "Test-SendMessageToAllUsersInTenantAsync":
                        await CallSendMessageToAllUsersInTenantAsync(turnContext);
                        break;
                    case "Test-SendMessageToAllUsersInTeamAsync":
                        await CallSendMessageToAllUsersInTeamAsync(turnContext);
                        break;
                    case "Test-SendMessageToListOfChannelsAsync":
                        await CallSendMessageToListOfChannelsAsync(turnContext);
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
                var creds = new MicrosoftAppCredentials(string.Empty, string.Empty);
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

                Assert.Equal("Organizer", participant.Meeting.Role);
                Assert.Equal("meetigConversationId-1", participant.Conversation.Id);
                Assert.Equal("userPrincipalName-1", participant.User.UserPrincipalName);
            }

            private async Task CallTeamsInfoGetMeetingInfoAsync(ITurnContext turnContext)
            {
                var meeting = await TeamsInfo.GetMeetingInfoAsync(turnContext);

                Assert.Equal("meeting-id", meeting.Details.Id);
                Assert.Equal("organizer-id", meeting.Organizer.Id);
                Assert.Equal("meetingConversationId-1", meeting.Conversation.Id);
            }

            private MeetingNotificationBase GetTargetedMeetingNotification(ChannelAccount from)
            {
                var recipients = new List<string> { from.Id };

                if (from.Name == "207")
                {
                    recipients.Add("failingid");
                }

                var meetingStageSurface = new MeetingStageSurface<TaskModuleContinueResponse>
                {
                    Content = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Title = "title here",
                            Height = 3,
                            Width = 2,
                        }
                    },
                    ContentType = ContentType.Task
                };

                var meetingTabIconSurface = new MeetingTabIconSurface
                {
                    TabEntityId = "test tab entity id"
                };

                var value = new TargetedMeetingNotificationValue
                {
                    Recipients = recipients,
                    Surfaces = new List<Surface> { meetingStageSurface, meetingTabIconSurface }
                };

                var obo = new OnBehalfOf
                {
                    DisplayName = from.Name,
                    Mri = from.Id
                };

                var channelData = new MeetingNotificationChannelData
                {
                    OnBehalfOfList = new[] { obo }
                };

                return new TargetedMeetingNotification
                {
                    Value = value,
                    ChannelData = channelData
                };
            }

            private async Task CallSendMeetingNotificationAsync(ITurnContext turnContext)
            {
                var from = turnContext.Activity.From;

                try
                {
                    var failedParticipants = await TeamsInfo.SendMeetingNotificationAsync(turnContext, GetTargetedMeetingNotification(from), "meeting-id").ConfigureAwait(false);

                    switch (from.Name)
                    {
                        case "207":
                            Assert.Equal("failingid", failedParticipants.RecipientsFailureInfo.First().RecipientMri);
                            break;
                        case "202":
                            Assert.Null(failedParticipants);
                            break;
                        default:
                            throw new InvalidOperationException($"Expected {nameof(HttpOperationException)} with response status code {from.Name}.");
                    }
                }
                catch (HttpOperationException ex)
                {
                    Assert.Equal(from.Name, ((int)ex.Response.StatusCode).ToString());
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(ex.Response.Content);

                    switch (from.Name)
                    {
                        case "400":
                            Assert.Equal("BadSyntax", errorResponse.Error.Code);
                            break;
                        case "403":
                            Assert.Equal("BotNotInConversationRoster", errorResponse.Error.Code);
                            break;
                        default:
                            throw new InvalidOperationException($"Expected {nameof(HttpOperationException)} with response status code {from.Name}.");
                    }
                }
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

            private async Task CallSendMessageToListOfUsersAsync(ITurnContext turnContext)
            {
                var from = turnContext.Activity.From;
                var members = new List<object>()
                {
                    new { Id = "member-1" },
                    new { Id = "member-2" },
                    new { Id = "member-3" },
                };
                var tenantId = "tenant-id";

                try
                {
                    var operationId = await TeamsInfo.SendMessageToListOfUsersAsync(turnContext, turnContext.Activity, members, tenantId).ConfigureAwait(false);

                    switch (from.Name)
                    {
                        case "201":
                            Assert.Equal("operation-1", operationId);
                            break;
                        default:
                            throw new InvalidOperationException($"Expected {nameof(HttpOperationException)} with response status code {from.Name}.");
                    }
                }
                catch (AggregateException ex)
                {
                    var firstException = ex.InnerExceptions.First();
                    var httpException = new HttpOperationException();
                    var errorResponse = new ErrorResponse();

                    switch (from.Name)
                    {
                        case "400":
                            Assert.Single(ex.InnerExceptions);
                            httpException = (HttpOperationException)firstException;
                            errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(httpException.Response.Content.ToString());
                            Assert.Equal("BadSyntax", errorResponse.Error.Code);
                            break;
                        case "403":
                            Assert.Single(ex.InnerExceptions);
                            httpException = (HttpOperationException)firstException;
                            errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(httpException.Response.Content.ToString());
                            Assert.Equal("Forbidden", errorResponse.Error.Code);
                            break;
                        case "429":
                            Assert.Equal(11, ex.InnerExceptions.Count);
                            break;
                        default:
                            throw new InvalidOperationException($"Expected {nameof(HttpOperationException)} with response status code {from.Name}.");
                    }
                }
            }

            private async Task CallSendMessageToAllUsersInTenantAsync(ITurnContext turnContext)
            {
                var from = turnContext.Activity.From;
                var tenantId = "tenant-id";

                try
                {
                    var operationId = await TeamsInfo.SendMessageToAllUsersInTenantAsync(turnContext, turnContext.Activity, tenantId).ConfigureAwait(false);

                    switch (from.Name)
                    {
                        case "201":
                            Assert.Equal("operation-1", operationId);
                            break;
                        default:
                            throw new InvalidOperationException($"Expected {nameof(HttpOperationException)} with response status code {from.Name}.");
                    }
                }
                catch (AggregateException ex)
                {
                    var firstException = ex.InnerExceptions.First();
                    var httpException = new HttpOperationException();
                    var errorResponse = new ErrorResponse();

                    switch (from.Name)
                    {
                        case "400":
                            Assert.Single(ex.InnerExceptions);
                            httpException = (HttpOperationException)firstException;
                            errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(httpException.Response.Content.ToString());
                            Assert.Equal("BadSyntax", errorResponse.Error.Code);
                            break;
                        case "403":
                            Assert.Single(ex.InnerExceptions);
                            httpException = (HttpOperationException)firstException;
                            errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(httpException.Response.Content.ToString());
                            Assert.Equal("Forbidden", errorResponse.Error.Code);
                            break;
                        case "429":
                            Assert.Equal(11, ex.InnerExceptions.Count);
                            break;
                        default:
                            throw new InvalidOperationException($"Expected {nameof(HttpOperationException)} with response status code {from.Name}.");
                    }
                }
            }

            private async Task CallSendMessageToAllUsersInTeamAsync(ITurnContext turnContext)
            {
                var from = turnContext.Activity.From;
                var teamId = "team-id";
                var tenantId = "tenant-id";

                try
                {
                    var operationId = await TeamsInfo.SendMessageToAllUsersInTeamAsync(turnContext, turnContext.Activity, teamId, tenantId).ConfigureAwait(false);

                    switch (from.Name)
                    {
                        case "201":
                            Assert.Equal("operation-1", operationId);
                            break;
                        default:
                            throw new InvalidOperationException($"Expected {nameof(HttpOperationException)} with response status code {from.Name}.");
                    }
                }
                catch (AggregateException ex)
                {
                    var firstException = ex.InnerExceptions.First();
                    var httpException = new HttpOperationException();
                    var errorResponse = new ErrorResponse();

                    switch (from.Name)
                    {
                        case "400":
                            Assert.Single(ex.InnerExceptions);
                            httpException = (HttpOperationException)firstException;
                            errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(httpException.Response.Content.ToString());
                            Assert.Equal("BadSyntax", errorResponse.Error.Code);
                            break;
                        case "403":
                            Assert.Single(ex.InnerExceptions);
                            httpException = (HttpOperationException)firstException;
                            errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(httpException.Response.Content.ToString());
                            Assert.Equal("Forbidden", errorResponse.Error.Code);
                            break;
                        case "404":
                            Assert.Single(ex.InnerExceptions);
                            httpException = (HttpOperationException)firstException;
                            errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(httpException.Response.Content.ToString());
                            Assert.Equal("NotFound", errorResponse.Error.Code);
                            break;
                        case "429":
                            Assert.Equal(11, ex.InnerExceptions.Count);
                            break;
                        default:
                            throw new InvalidOperationException($"Expected {nameof(HttpOperationException)} with response status code {from.Name}.");
                    }
                }
            }
            
            private async Task CallSendMessageToListOfChannelsAsync(ITurnContext turnContext)
            {
                var from = turnContext.Activity.From;
                var members = new List<object>()
                {
                    new { Id = "channel-1" },
                    new { Id = "channel-2" },
                    new { Id = "channel-3" },
                };
                var tenantId = "tenant-id";

                try
                {
                    var operationId = await TeamsInfo.SendMessageToListOfChannelsAsync(turnContext, turnContext.Activity, members, tenantId).ConfigureAwait(false);

                    switch (from.Name)
                    {
                        case "201":
                            Assert.Equal("operation-1", operationId);
                            break;
                        default:
                            throw new InvalidOperationException($"Expected {nameof(HttpOperationException)} with response status code {from.Name}.");
                    }
                }
                catch (AggregateException ex)
                {
                    var firstException = ex.InnerExceptions.First();
                    var httpException = new HttpOperationException();
                    var errorResponse = new ErrorResponse();

                    switch (from.Name)
                    {
                        case "400":
                            Assert.Single(ex.InnerExceptions);
                            httpException = (HttpOperationException)firstException;
                            errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(httpException.Response.Content.ToString());
                            Assert.Equal("BadSyntax", errorResponse.Error.Code);
                            break;
                        case "403":
                            Assert.Single(ex.InnerExceptions);
                            httpException = (HttpOperationException)firstException;
                            errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(httpException.Response.Content.ToString());
                            Assert.Equal("Forbidden", errorResponse.Error.Code);
                            break;
                        case "429":
                            Assert.Equal(11, ex.InnerExceptions.Count);
                            break;
                        default:
                            throw new InvalidOperationException($"Expected {nameof(HttpOperationException)} with response status code {from.Name}.");
                    }
                }
            }
        }

        private class RosterHttpMessageHandler : HttpMessageHandler
        {
            protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
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
                            new JProperty("user", new JObject(new JProperty("userPrincipalName", "userPrincipalName-1"))),
                            new JProperty("meeting", new JObject(new JProperty("role", "Organizer"))),
                            new JProperty("conversation", new JObject(new JProperty("Id", "meetigConversationId-1"))),
                        };
                    response.Content = new StringContent(content.ToString());
                }

                // Get meeting details
                else if (request.RequestUri.PathAndQuery.EndsWith("v1/meetings/meeting-id"))
                {
                    var content = new JObject
                        {
                            new JProperty("details", new JObject(new JProperty("id", "meeting-id"))),
                            new JProperty("organizer", new JObject(new JProperty("id", "organizer-id"))),
                            new JProperty("conversation", new JObject(new JProperty("id", "meetingConversationId-1"))),
                        };
                    response.Content = new StringContent(content.ToString());
                }

                // Post meeting notification
                else if (request.RequestUri.PathAndQuery.EndsWith("v1/meetings/meeting-id/notification"))
                {
                    var responseBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var notification = JsonConvert.DeserializeObject<TargetedMeetingNotification>(responseBody);
                    var obo = notification.ChannelData.OnBehalfOfList.First();

                    // hack displayname as expected status code, for testing
                    switch (obo.DisplayName)
                    {
                        case "207":
                            var failureInfo = new MeetingNotificationRecipientFailureInfo { RecipientMri = notification.Value.Recipients.First(r => !r.Equals(obo.Mri, StringComparison.OrdinalIgnoreCase)) };
                            var infos = new MeetingNotificationResponse
                            {
                                RecipientsFailureInfo = new List<MeetingNotificationRecipientFailureInfo> { failureInfo }
                            };

                            response.Content = new StringContent(JsonConvert.SerializeObject(infos));
                            response.StatusCode = HttpStatusCode.MultiStatus;
                            break;
                        case "403":
                            response.Content = new StringContent("{\"error\":{\"code\":\"BotNotInConversationRoster\"}}");
                            response.StatusCode = HttpStatusCode.Forbidden;
                            break;
                        case "400":
                            response.Content = new StringContent("{\"error\":{\"code\":\"BadSyntax\"}}");
                            response.StatusCode = HttpStatusCode.BadRequest;
                            break;
                        default:
                            response.StatusCode = HttpStatusCode.Accepted;
                            break;
                    }
                }

                // SendMessageToListOfUsers
                else if (request.RequestUri.PathAndQuery.EndsWith("v3/batch/conversation/users/"))
                {
                    var requestBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var payload = JObject.Parse(requestBody);
                    var requestActivity = payload.SelectToken("Activity").ToObject<Activity>();

                    // hack From.Name as expected status code, for testing
                    switch (requestActivity.From.Name)
                    {
                        case "201":
                            response.Content = new StringContent("operation-1");
                            response.StatusCode = HttpStatusCode.Created;
                            break;
                        case "400":
                            response.Content = new StringContent("{\"error\":{\"code\":\"BadSyntax\"}}");
                            response.StatusCode = HttpStatusCode.BadRequest;
                            break;
                        case "403":
                            response.Content = new StringContent("{\"error\":{\"code\":\"Forbidden\"}}");
                            response.StatusCode = HttpStatusCode.Forbidden;
                            break;
                        case "429":
                            response.Content = new StringContent("{\"error\":{\"code\":\"TooManyRequests\"}}");
                            response.StatusCode = HttpStatusCode.TooManyRequests;
                            break;
                        default:
                            response.StatusCode = HttpStatusCode.Accepted;
                            break;
                    }
                }
                
                // SendMessageToAllUsersInTenant
                else if (request.RequestUri.PathAndQuery.EndsWith("v3/batch/conversation/tenant/"))
                {
                    var requestBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var payload = JObject.Parse(requestBody);
                    var requestActivity = payload.SelectToken("Activity").ToObject<Activity>();

                    // hack From.Name as expected status code, for testing
                    switch (requestActivity.From.Name)
                    {
                        case "201":
                            response.Content = new StringContent("operation-1");
                            response.StatusCode = HttpStatusCode.Created;
                            break;
                        case "400":
                            response.Content = new StringContent("{\"error\":{\"code\":\"BadSyntax\"}}");
                            response.StatusCode = HttpStatusCode.BadRequest;
                            break;
                        case "403":
                            response.Content = new StringContent("{\"error\":{\"code\":\"Forbidden\"}}");
                            response.StatusCode = HttpStatusCode.Forbidden;
                            break;
                        case "429":
                            response.Content = new StringContent("{\"error\":{\"code\":\"TooManyRequests\"}}");
                            response.StatusCode = HttpStatusCode.TooManyRequests;
                            break;
                        default:
                            response.StatusCode = HttpStatusCode.Accepted;
                            break;
                    }
                }
                // SendMessageToAllUsersInTeam
                else if (request.RequestUri.PathAndQuery.EndsWith("v3/batch/conversation/team/"))
                {
                    var requestBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var payload = JObject.Parse(requestBody);
                    var requestActivity = payload.SelectToken("Activity").ToObject<Activity>();

                    // hack From.Name as expected status code, for testing
                    switch (requestActivity.From.Name)
                    {
                        case "201":
                            response.Content = new StringContent("operation-1");
                            response.StatusCode = HttpStatusCode.Created;
                            break;
                        case "400":
                            response.Content = new StringContent("{\"error\":{\"code\":\"BadSyntax\"}}");
                            response.StatusCode = HttpStatusCode.BadRequest;
                            break;
                        case "403":
                            response.Content = new StringContent("{\"error\":{\"code\":\"Forbidden\"}}");
                            response.StatusCode = HttpStatusCode.Forbidden;
                            break;
                        case "404":
                            response.Content = new StringContent("{\"error\":{\"code\":\"NotFound\"}}");
                            response.StatusCode = HttpStatusCode.NotFound;
                            break;
                        case "429":
                            response.Content = new StringContent("{\"error\":{\"code\":\"TooManyRequests\"}}");
                            response.StatusCode = HttpStatusCode.TooManyRequests;
                            break;
                        default:
                            response.StatusCode = HttpStatusCode.Accepted;
                            break;
                    }
                }
                // SendMessageToListOfChannels
                else if (request.RequestUri.PathAndQuery.EndsWith("v3/batch/conversation/channels/"))
                {
                    var requestBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var payload = JObject.Parse(requestBody);
                    var requestActivity = payload.SelectToken("Activity").ToObject<Activity>();

                    // hack From.Name as expected status code, for testing
                    switch (requestActivity.From.Name)
                    {
                        case "201":
                            response.Content = new StringContent("operation-1");
                            response.StatusCode = HttpStatusCode.Created;
                            break;
                        case "400":
                            response.Content = new StringContent("{\"error\":{\"code\":\"BadSyntax\"}}");
                            response.StatusCode = HttpStatusCode.BadRequest;
                            break;
                        case "403":
                            response.Content = new StringContent("{\"error\":{\"code\":\"Forbidden\"}}");
                            response.StatusCode = HttpStatusCode.Forbidden;
                            break;
                        case "429":
                            response.Content = new StringContent("{\"error\":{\"code\":\"TooManyRequests\"}}");
                            response.StatusCode = HttpStatusCode.TooManyRequests;
                            break;
                        default:
                            response.StatusCode = HttpStatusCode.Accepted;
                            break;
                    }
                }

                return response;
            }
        }

        private class TestCreateConversationAdapter : BotAdapter
        {
            private string _activityId;

            private string _conversationId;

            public TestCreateConversationAdapter(string activityId, string conversationId)
            {
                _activityId = activityId;
                _conversationId = conversationId;
            }

            public string AppId { get; set; }

            public string ChannelId { get; set; }

            public string ServiceUrl { get; set; }

            public string Audience { get; set; }

            public ConversationParameters ConversationParameters { get; set; }

            public override Task CreateConversationAsync(string botAppId, string channelId, string serviceUrl, string audience, ConversationParameters conversationParameters, BotCallbackHandler callback, CancellationToken cancellationToken)
            {
                AppId = botAppId;
                ChannelId = channelId;
                ServiceUrl = serviceUrl;
                Audience = audience;
                ConversationParameters = conversationParameters;

                var activity = new Activity { Id = _activityId, Conversation = new ConversationAccount { Id = _conversationId } };

                var mockTurnContext = new Mock<ITurnContext>();
                mockTurnContext.Setup(tc => tc.Activity).Returns(activity);

                callback(mockTurnContext.Object, cancellationToken);
                return Task.CompletedTask;
            }

            public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
