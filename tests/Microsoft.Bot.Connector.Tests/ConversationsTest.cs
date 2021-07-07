// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Xunit;

namespace Microsoft.Bot.Connector.Tests
{
    public class ConversationsTest : BaseTest
    {
        [Fact]
        public void AgentStringComponents()
        {
            var aspNet = ConnectorClient.GetASPNetVersion();
            var arch = ConnectorClient.GetArchitecture();
            var os = ConnectorClient.GetOsVersion();
        }

        [Fact]
        public void Constructor_Guard()
            => Assert.Throws<System.ArgumentNullException>("client", () => new Conversations(null));

        [Fact]
        public async Task CreateConversation()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Create Conversation",
            };

            var param = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var result = await client.Conversations.CreateConversationAsync(param);
                Assert.NotNull(result.ActivityId);
            });
        }

        [Fact]
        public async Task CreateConversation_WithTracing()
            => await AssertTracingFor(CreateConversation, nameof(ConversationsExtensions.CreateConversationAsync));

        [Fact]
        public async Task CreateConversationWithInvalidBot()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Create Conversation with invalid Bot",
            };

            var param = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = new ChannelAccount() { Id = "invalid-id" },
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Conversations.CreateConversationAsync(param));
                Assert.Equal("ServiceError", ex.Body.Error.Code);
                Assert.StartsWith("Invalid userId", ex.Body.Error.Message);
            });
        }

        [Fact]
        public async Task CreateConversationWithInvalidBot_WithTracing()
            => await AssertTracingFor(CreateConversationWithInvalidBot, nameof(ConversationsExtensions.CreateConversationAsync), isSuccesful: false);

        [Fact]
        public async Task CreateConversationWithoutMembers()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Create Conversation without members",
            };

            var param = new ConversationParameters()
            {
                Members = new ChannelAccount[] { },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Conversations.CreateConversationAsync(param));
                Assert.Equal("BadArgument", ex.Body.Error.Code);
                Assert.StartsWith("Conversations", ex.Body.Error.Message);
            });
        }

        [Fact]
        public async Task CreateConversationWithoutMembers_WithTracing()
            => await AssertTracingFor(CreateConversationWithoutMembers, nameof(ConversationsExtensions.CreateConversationAsync), isSuccesful: false);

        [Fact]
        public async Task CreateConversationWithBotMember()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Create Conversation with Bot member",
            };

            var param = new ConversationParameters()
            {
                Members = new ChannelAccount[] { Bot },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Conversations.CreateConversationAsync(param));
                Assert.Equal("BadArgument", ex.Body.Error.Code);
            });
        }

        [Fact]
        public async Task CreateConversationWithBotMember_WithTracing()
            => await AssertTracingFor(CreateConversationWithBotMember, nameof(ConversationsExtensions.CreateConversationAsync), isSuccesful: false);

        [Fact]
        public async Task CreateConversationWithNullParameter()
        {
            await UseClientFor(async client =>
            {
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.CreateConversationAsync(null));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task CreateConversationWithCustomHeader()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Create Conversation",
            };

            var param = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            var customHeaders = new Dictionary<string, List<string>>() { { "customHeader", new List<string>() { "customValue" } } };

            await AssertTracingFor(
                async () =>
                await UseClientFor(async client =>
                {
                    var result = await client.Conversations.CreateConversationWithHttpMessagesAsync(param, customHeaders, default(CancellationToken));
                    Assert.NotNull(result.Body);
                    Assert.NotNull(result.Body.ActivityId);
                }),
                nameof(ConversationsExtensions.CreateConversationAsync),
                assertHttpRequestMessage:
                (h) => h.Headers.Contains("customHeader") && h.Headers.GetValues("customHeader").Contains("customValue"));
        }

        [Fact]
        public async Task GetConversationMembers()
        {
            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var members = await client.Conversations.GetConversationMembersAsync(conversation.Id);

                var hasUser = false;

                foreach (var member in members)
                {
                    hasUser = member.Id == User.Id;
                    if (hasUser)
                    {
                        break;
                    }
                }

                Assert.True(hasUser);
            });
        }

        [Fact]
        public async Task GetConversationMembers_WithTracing()
            => await AssertTracingFor(GetConversationMembers, nameof(ConversationsExtensions.GetConversationMembersAsync));

        [Fact]
        public async Task GetConversationMembersWithInvalidConversationId()
        {
            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Conversations.GetConversationMembersAsync(string.Concat(conversation.Id, "M")));
                Assert.Equal("BadArgument", ex.Body.Error.Code);
                Assert.Contains("Slack API error", ex.Body.Error.Message);
            });
        }

        [Fact]
        public async Task GetConversationMembersWithInvalidConversationId_WithTracing()
            => await AssertTracingFor(GetConversationMembersWithInvalidConversationId, nameof(ConversationsExtensions.GetConversationMembersAsync), isSuccesful: false);

        [Fact]
        public async Task GetConversationMembersWithNullConversationId()
        {
            await UseClientFor(async client =>
            {
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.GetConversationMembersAsync(null));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task GetConversationMembersWithCustomHeader()
        {
            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            var customHeaders = new Dictionary<string, List<string>>() { { "customHeader", new List<string>() { "customValue" } } };

            await AssertTracingFor(
                async () =>
                    await UseClientFor(async client =>
                                {
                                    var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                                    var members = await client.Conversations.GetConversationMembersWithHttpMessagesAsync(conversation.Id, customHeaders, default(CancellationToken));

                                    var hasUser = false;

                                    foreach (var member in members.Body)
                                    {
                                        hasUser = member.Id == User.Id;
                                        if (hasUser)
                                        {
                                            break;
                                        }
                                    }

                                    Assert.True(hasUser);
                                }),
                nameof(ConversationsExtensions.GetConversationMembersAsync),
                assertHttpRequestMessage:
                    (h) => h.Headers.Contains("customHeader") && h.Headers.GetValues("customHeader").Contains("customValue"));
        }

        [Fact]
        public async Task GetConversationPagedMembers()
        {
            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var membersResult = await client.Conversations.GetConversationPagedMembersAsync(conversation.Id);

                var hasUser = false;

                foreach (var member in membersResult.Members)
                {
                    hasUser = member.Id == User.Id;
                    if (hasUser)
                    {
                        break;
                    }
                }

                Assert.True(hasUser);
            });
        }

        [Fact]
        public async Task GetConversationPagedMembers_WithTracing()
            => await AssertTracingFor(GetConversationPagedMembers, nameof(ConversationsExtensions.GetConversationPagedMembersAsync));

        [Fact]
        public async Task GetConversationPagedMembersWithInvalidConversationId()
        {
            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var ex = await Assert.ThrowsAsync<HttpOperationException>(() => client.Conversations.GetConversationPagedMembersAsync(string.Concat(conversation.Id, "M")));
                Assert.Equal(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
            });
        }

        [Fact]
        public async Task GetConversationPagedMembersWithInvalidConversationId_WithTracing()
            => await AssertTracingFor(GetConversationPagedMembersWithInvalidConversationId, nameof(ConversationsExtensions.GetConversationPagedMembersAsync), isSuccesful: false);

        [Fact]
        public async Task GetConversationPagedMembersWithPageSize()
        {
            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var membersResult = await client.Conversations.GetConversationPagedMembersAsync(conversation.Id, pageSize: 10);

                var hasUser = false;

                foreach (var member in membersResult.Members)
                {
                    hasUser = member.Id == User.Id;
                    if (hasUser)
                    {
                        break;
                    }
                }

                Assert.True(hasUser);
            });
        }

        [Fact]
        public async Task GetConversationPagedMembersWithPageSize_WithTracing()
            => await AssertTracingFor(GetConversationPagedMembersWithPageSize, nameof(ConversationsExtensions.GetConversationPagedMembersAsync));

        [Fact]
        public async Task GetConversationPagedMembersWithCustomHeaders()
        {
            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            var customHeaders = new Dictionary<string, List<string>>() { { "customHeader", new List<string>() { "customValue" } } };

            await AssertTracingFor(
                async () =>
                await UseClientFor(async client =>
                {
                    var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                    var members = await client.Conversations.GetConversationPagedMembersWithHttpMessagesAsync(
                        conversation.Id, customHeaders: customHeaders, cancellationToken: default(CancellationToken));

                    var hasUser = false;

                    foreach (var member in members.Body.Members)
                    {
                        hasUser = member.Id == User.Id;
                        if (hasUser)
                        {
                            break;
                        }
                    }

                    Assert.True(hasUser);
                }),
                nameof(ConversationsExtensions.GetConversationPagedMembersAsync),
                assertHttpRequestMessage:
                    (h) => h.Headers.Contains("customHeader") && h.Headers.GetValues("customHeader").Contains("customValue"));
        }

        [Fact]
        public async Task SendToConversation()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Name = "acticity",
                Text = "TEST Send to Conversation",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var response = await client.Conversations.SendToConversationAsync(conversation.Id, activity);
                Assert.NotNull(response.Id);
            });
        }

        [Fact]
        public async Task SendToConversation_WithTracing()
            => await AssertTracingFor(SendToConversation, nameof(ConversationsExtensions.SendToConversationAsync));

        [Fact]
        public async Task SendToConversationWithInvalidConversationId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Name = "acticity",
                Text = "TEST Send to Conversation with invalid conversation id",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Conversations.SendToConversationAsync(conversationId: string.Concat(conversation.Id, "M"), activity: activity));
                Assert.Equal("BadArgument", ex.Body.Error.Code);
                Assert.Contains("Slack API error", ex.Body.Error.Message);
            });
        }

        [Fact]
        public async Task SendToConversationWithInvalidConversationId_WithTracing()
            => await AssertTracingFor(SendToConversationWithInvalidConversationId, nameof(ConversationsExtensions.SendToConversationAsync), isSuccesful: false);

        [Fact]
        public async Task SendToConversationWithInvalidBotId()
        {
            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var activity = new Activity()
                {
                    Type = ActivityTypes.Message,
                    Recipient = User,
                    From = new ChannelAccount() { Id = "B21S8SG7K:T03CWQ0QB" },
                    Name = "acticity",
                    Text = "TEST Send to Conversation",
                };
                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Conversations.SendToConversationAsync(conversationId: string.Concat(conversation.Id, "M"), activity: activity));
                Assert.Equal("Operation returned an invalid status code 'Forbidden'", ex.Message);
            });
        }

        [Fact]
        public async Task SendToConversationWithInvalidBotId_WithTracing()
            => await AssertTracingFor(SendToConversationWithInvalidBotId, nameof(ConversationsExtensions.SendToConversationAsync), isSuccesful: false);

        [Fact]
        public async Task SendToConversationWithNullConversationId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Name = "acticity",
                Text = "TEST Send to Conversation with null conversation id",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.SendToConversationAsync(conversationId: null, activity: activity));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task SendToConversationWithNullActivity()
        {
            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: null));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task SendCardToConversation()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Name = "acticity",
                Text = "TEST Send Card to Conversation",
                Attachments = new Attachment[]
                {
                    new Attachment()
                    {
                        ContentType = HeroCard.ContentType,
                        Content = new HeroCard()
                        {
                            Title = "A static image",
                            Subtitle = "JPEG image",
                            Images = new CardImage[] { new CardImage() { Url = "https://docs.microsoft.com/bot-framework/media/designing-bots/core/dialogs-screens.png" } },
                        },
                    },
                    new Attachment()
                    {
                        ContentType = HeroCard.ContentType,
                        Content = new HeroCard()
                        {
                            Title = "An animation",
                            Subtitle = "GIF image",
                            Images = new CardImage[] { new CardImage() { Url = "http://i.giphy.com/Ki55RUbOV5njy.gif" } },
                        },
                    },
                },
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var response = await client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: activity);
                Assert.NotNull(response.Id);
            });
        }

        [Fact]
        public async Task SendCardToConversation_WithTracing()
            => await AssertTracingFor(SendCardToConversation, nameof(ConversationsExtensions.SendToConversationAsync));

        [Fact]
        public async Task SendToConversationWithCustomHeaders()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Name = "acticity",
                Text = "TEST Send to Conversation",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            var customHeaders = new Dictionary<string, List<string>>() { { "customHeader", new List<string>() { "customValue" } } };

            await AssertTracingFor(
                async () =>
                await UseClientFor(async client =>
                {
                    var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                    var response = await client.Conversations.SendToConversationWithHttpMessagesAsync(conversation.Id, activity, customHeaders, default(CancellationToken));
                    Assert.NotNull(response.Body.Id);
                }),
                nameof(ConversationsExtensions.SendToConversationAsync),
                assertHttpRequestMessage:
                    (h) => h.Headers.Contains("customHeader") && h.Headers.GetValues("customHeader").Contains("customValue"));
        }

        [Fact]
        public async Task GetActivityMembers()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Get Activity Members",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var members = await client.Conversations.GetActivityMembersAsync(conversation.Id, conversation.ActivityId);

                var hasUser = false;

                foreach (var member in members)
                {
                    hasUser = member.Id == User.Id;
                    if (hasUser)
                    {
                        break;
                    }
                }

                Assert.True(hasUser);
            });
        }

        [Fact]
        public async Task GetActivityMembers_WithTracing()
            => await AssertTracingFor(GetActivityMembers, nameof(ConversationsExtensions.GetActivityMembersAsync));

        [Fact]
        public async Task GetActivityMembersWithInvalidConversationId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Get Activity Members with invalid conversation id",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Conversations.GetActivityMembersAsync(string.Concat(conversation.Id, "M"), conversation.ActivityId));
                Assert.Equal("BadArgument", ex.Body.Error.Code);
                Assert.Contains("Slack API error", ex.Body.Error.Message);
            });
        }

        [Fact]
        public async Task GetActivityMembersWithInvalidConversationId_WithTracing()
            => await AssertTracingFor(GetActivityMembersWithInvalidConversationId, nameof(ConversationsExtensions.GetActivityMembersAsync), isSuccesful: false);

        [Fact]
        public async Task GetActivityMembersWithNullConversationId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Get Activity Members with null conversation id",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.GetActivityMembersAsync(null, conversation.ActivityId));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task GetActivityMembersWithNullActivityId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Get Activity Members with null activity id",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.GetActivityMembersAsync(conversation.Id, null));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task GetActivityMembersWithCustomHeaders()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Get Activity Members",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            var customHeaders = new Dictionary<string, List<string>>() { { "customHeader", new List<string>() { "customValue" } } };

            await AssertTracingFor(
                async () =>
                    await UseClientFor(async client =>
                    {
                        var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                        var members = await client.Conversations.GetActivityMembersWithHttpMessagesAsync(
                            conversation.Id, conversation.ActivityId, customHeaders, default(CancellationToken));

                        var hasUser = false;

                        foreach (var member in members.Body)
                        {
                            hasUser = member.Id == User.Id;
                            if (hasUser)
                            {
                                break;
                            }
                        }

                        Assert.True(hasUser);
                    }),
                nameof(ConversationsExtensions.GetActivityMembersAsync),
                assertHttpRequestMessage:
                        (h) => h.Headers.Contains("customHeader") && h.Headers.GetValues("customHeader").Contains("customValue"));
        }

        [Fact]
        public async Task ReplyToActivity()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Activity gets a reply",
            };

            var reply = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Reply to Activity",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var response = await client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: activity);
                var replyResponse = await client.Conversations.ReplyToActivityAsync(conversation.Id, response.Id, reply);

                Assert.NotNull(replyResponse.Id);
            });
        }

        [Fact]
        public async Task ReplyToActivity_WithTracing()
            => await AssertTracingFor(ReplyToActivity, nameof(ConversationsExtensions.ReplyToActivityAsync));

        [Fact]
        public async Task ReplyToActivityWithInvalidConversationId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Reply activity with invalid conversation id",
            };

            var reply = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Reply mustn't shown",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var response = await client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: activity);
                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Conversations.ReplyToActivityAsync(string.Concat(conversation.Id, "M"), response.Id, reply));
                Assert.Equal("BadArgument", ex.Body.Error.Code);
                Assert.Contains("Slack API error", ex.Body.Error.Message);
            });
        }

        [Fact]
        public async Task ReplyToActivityWithInvalidConversationId_WithTracing()
            => await AssertTracingFor(ReplyToActivityWithInvalidConversationId, nameof(ConversationsExtensions.ReplyToActivityAsync), isSuccesful: false);

        [Fact]
        public async Task ReplyToActivityWithNullConversationId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Reply activity with null conversation id",
            };

            var reply = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Reply mustn't shown",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var response = await client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: activity);
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.ReplyToActivityAsync(null, response.Id, reply));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task ReplyToActivityWithNullActivityId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Reply activity with null activity id",
            };

            var reply = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Reply mustn't shown",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var response = await client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: activity);
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.ReplyToActivityAsync(conversation.Id, null, reply));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task ReplyToActivityWithNullReply()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Reply activity with null reply",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var response = await client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: activity);
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.ReplyToActivityAsync(conversation.Id, response.Id, null));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task ReplyToActivityWithCustomHeaders()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Activity gets a reply",
            };

            var reply = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Reply to Activity",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            var customHeaders = new Dictionary<string, List<string>>() { { "customHeader", new List<string>() { "customValue" } } };

            var conversationId = string.Empty;

            await AssertTracingFor(
                async () =>
                await UseClientFor(async client =>
                {
                    var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                    conversationId = conversation.Id;
                    var response = await client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: activity);
                    var replyResponse = await client.Conversations.ReplyToActivityWithHttpMessagesAsync(
                        conversation.Id, response.Id, reply, customHeaders, default(CancellationToken));

                    Assert.NotNull(replyResponse.Body.Id);
                }),
                nameof(ConversationsExtensions.ReplyToActivityAsync),
                assertHttpRequestMessage:
                    (h) =>
                    {
                        bool customHeaderAssertion = h.Headers.Contains("customHeader") &&
                                                     h.Headers.GetValues("customHeader").Contains("customValue");
                        bool convIdHeaderAssertion = h.Headers.Contains(ConversationConstants.ConversationIdHttpHeaderName) &&
                                                     h.Headers.GetValues(ConversationConstants.ConversationIdHttpHeaderName).Contains(conversationId);

                        return customHeaderAssertion && convIdHeaderAssertion;
                    });
        }

        [Fact]
        public async Task DeleteActivity()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Activity to be deleted",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                await client.Conversations.DeleteActivityAsync(conversation.Id, conversation.ActivityId);
                Assert.NotNull(conversation.ActivityId);
            });
        }

        [Fact]
        public async Task DeleteActivity_WithTracing()
            => await AssertTracingFor(DeleteActivity, nameof(ConversationsExtensions.DeleteActivityAsync));

        [Fact]
        public async Task DeleteActivityWithInvalidConversationId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Activity to be deleted with invalid conversation Id",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Conversations.DeleteActivityAsync("B21S8SG7K:T03CWQ0QB", conversation.ActivityId));
                Assert.Equal("ServiceError", ex.Body.Error.Code);
                Assert.Contains("Invalid ConversationId", ex.Body.Error.Message);
            });
        }

        [Fact]
        public async Task DeleteActivityWithInvalidConversationId_WithTracing()
            => await AssertTracingFor(DeleteActivityWithInvalidConversationId, nameof(ConversationsExtensions.DeleteActivityAsync), isSuccesful: false);

        [Fact]
        public async Task DeleteActivityWithNullConversationId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Activity to be deleted with null conversation Id",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.DeleteActivityAsync(null, conversation.ActivityId));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task DeleteActivityWithNullActivityId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Activity to be deleted with null activity Id",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.DeleteActivityAsync("B21S8SG7K:T03CWQ0QB", null));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task DeleteActivityWithCustomHeaders()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Activity to be deleted",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
                Activity = activity,
            };

            var customHeaders = new Dictionary<string, List<string>>() { { "customHeader", new List<string>() { "customValue" } } };

            await AssertTracingFor(
                async () =>
                    await UseClientFor(async client =>
                    {
                        var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                        await client.Conversations.DeleteActivityWithHttpMessagesAsync(
                            conversation.Id, conversation.ActivityId, customHeaders, default(CancellationToken));
                        Assert.NotNull(conversation.ActivityId);
                    }),
                nameof(ConversationsExtensions.DeleteActivityAsync),
                assertHttpRequestMessage:
                        (h) => h.Headers.Contains("customHeader") && h.Headers.GetValues("customHeader").Contains("customValue"));
        }

        [Fact]
        public async Task UpdateActivity()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Activity to be updated",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var response = await client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: activity);
                var update = new Activity()
                {
                    Id = response.Id,
                    Type = ActivityTypes.Message,
                    Recipient = User,
                    From = Bot,
                    Text = "TEST Successfully activity updated",
                };
                var updateResponse = await client.Conversations.UpdateActivityAsync(conversation.Id, response.Id, update);
                Assert.NotNull(updateResponse.Id);
            });
        }

        [Fact]
        public async Task UpdateActivity_WithTracing()
            => await AssertTracingFor(UpdateActivity, nameof(ConversationsExtensions.UpdateActivity));

        [Fact]
        public async Task UpdateActivityWithInvalidConversationId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Activity to be updated with invalid conversation Id",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var response = await client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: activity);
                var update = new Activity()
                {
                    Id = response.Id,
                    Type = ActivityTypes.Message,
                    Recipient = User,
                    From = Bot,
                    Text = "TEST Activity mustn't be updated",
                };
                var ex = await Assert.ThrowsAsync<ErrorResponseException>(() => client.Conversations.UpdateActivityAsync("B21S8SG7K:T03CWQ0QB", response.Id, update));
                Assert.Equal("ServiceError", ex.Body.Error.Code);
                Assert.Contains("Invalid ConversationId", ex.Body.Error.Message);
            });
        }

        [Fact]
        public async Task UpdateActivityWithInvalidConversationId_WithTracing()
            => await AssertTracingFor(UpdateActivityWithInvalidConversationId, nameof(ConversationsExtensions.UpdateActivity), isSuccesful: false);

        [Fact]
        public async Task UpdateActivityWithNullConversationId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Activity to be updated with null conversation Id",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var response = await client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: activity);
                var update = new Activity()
                {
                    Id = response.Id,
                    Type = ActivityTypes.Message,
                    Recipient = User,
                    From = Bot,
                    Text = "TEST Activity mustn't be updated",
                };
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.UpdateActivityAsync(null, response.Id, update));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task UpdateActivityWithNullActivityId()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Activity to be updated with null activity Id",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var response = await client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: activity);
                var update = new Activity()
                {
                    Id = response.Id,
                    Type = ActivityTypes.Message,
                    Recipient = User,
                    From = Bot,
                    Text = "TEST Activity mustn't be updated",
                };
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.UpdateActivityAsync(conversation.Id, null, update));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task UpdateActivityWithNullActivity()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Activity to be updated with null activity",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            await UseClientFor(async client =>
            {
                var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                var response = await client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: activity);
                var ex = await Assert.ThrowsAsync<ValidationException>(() => client.Conversations.UpdateActivityAsync(conversation.Id, response.Id, null));
                Assert.Contains("cannot be null", ex.Message);
            });
        }

        [Fact]
        public async Task UpdateActivityWithCustomHeaders()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = User,
                From = Bot,
                Text = "TEST Activity to be updated",
            };

            var createMessage = new ConversationParameters()
            {
                Members = new ChannelAccount[] { User },
                Bot = Bot,
            };

            var customHeaders = new Dictionary<string, List<string>>() { { "customHeader", new List<string>() { "customValue" } } };

            await AssertTracingFor(
                async () =>
                await UseClientFor(async client =>
                {
                    var conversation = await client.Conversations.CreateConversationAsync(createMessage);
                    var response = await client.Conversations.SendToConversationAsync(conversationId: conversation.Id, activity: activity);
                    var update = new Activity()
                    {
                        Id = response.Id,
                        Type = ActivityTypes.Message,
                        Recipient = User,
                        From = Bot,
                        Text = "TEST Successfully activity updated",
                    };
                    var updateResponse = await client.Conversations.UpdateActivityWithHttpMessagesAsync(
                        conversation.Id, response.Id, update, customHeaders, default(CancellationToken));
                    Assert.NotNull(updateResponse.Body.Id);
                }),
                nameof(ConversationsExtensions.UpdateActivity),
                assertHttpRequestMessage:
                    (h) => h.Headers.Contains("customHeader") && h.Headers.GetValues("customHeader").Contains("customValue"));
        }
    }
}
