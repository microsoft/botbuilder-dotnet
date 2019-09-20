// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Moq;
using SlackAPI;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Slack.Tests
{
    public class SlackAdapterTests
    {
        [Fact]
        public void ConstructorShouldFailWithNullClient()
        {
            Assert.Throws<ArgumentNullException>(() => new SlackAdapter(null));
        }

        [Fact]
        public void ConstructorSucceeds()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new Mock<SlackClientWrapper>(options.Object);

            // TODO: delete when LoginWithSlack method gets removed from SlackAdapter.
            slackApi.Setup(x => x.TestAuthAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult("mockedUserId"));

            Assert.NotNull(new SlackAdapter(slackApi.Object));
        }

        [Fact]
        public async Task UpdateActivityAsyncShouldFailWithNullActivityTimestamp()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new Mock<SlackClientWrapper>(options.Object);

            // TODO: delete when LoginWithSlack method gets removed from SlackAdapter.
            slackApi.Setup(x => x.TestAuthAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult("mockedUserId"));

            var slackAdapter = new SlackAdapter(slackApi.Object);

            var activity = new Activity
            {
                Timestamp = null,
            };

            var turnContext = new TurnContext(slackAdapter, activity);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await slackAdapter.UpdateActivityAsync(turnContext, activity, default);
            });
        }

        [Fact]
        public async Task UpdateActivityAsyncShouldFailWithNullContext()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new Mock<SlackClientWrapper>(options.Object);

            // TODO: delete when LoginWithSlack method gets removed from SlackAdapter.
            slackApi.Setup(x => x.TestAuthAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult("mockedUserId"));

            var slackAdapter = new SlackAdapter(slackApi.Object);

            var activity = new Activity();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await slackAdapter.UpdateActivityAsync(null, activity, default);
            });
        }

        [Fact]
        public async Task UpdateActivityAsyncShouldFailWithNullActivity()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new Mock<SlackClientWrapper>(options.Object);

            // TODO: delete when LoginWithSlack method gets removed from SlackAdapter.
            slackApi.Setup(x => x.TestAuthAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult("mockedUserId"));

            var slackAdapter = new SlackAdapter(slackApi.Object);

            var turnContext = new TurnContext(slackAdapter, new Activity());

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await slackAdapter.UpdateActivityAsync(turnContext, null, default);
            });
        }

        [Fact]
        public async Task UpdateActivityAsyncShouldFailWithNullActivityConversation()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new Mock<SlackClientWrapper>(options.Object);

            // TODO: delete when LoginWithSlack method gets removed from SlackAdapter.
            slackApi.Setup(x => x.TestAuthAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult("mockedUserId"));

            var slackAdapter = new SlackAdapter(slackApi.Object);

            var activity = new Activity
            {
                Id = "testId",
                Conversation = null,
            };

            var turnContext = new TurnContext(slackAdapter, activity);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await slackAdapter.UpdateActivityAsync(turnContext, activity, default);
            });
        }

        [Fact]
        public async Task UpdateActivityAsyncShouldSucceed()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new Mock<SlackClientWrapper>(options.Object);

            // TODO: delete when LoginWithSlack method gets removed from SlackAdapter.
            slackApi.Setup(x => x.TestAuthAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult("mockedUserId"));
            slackApi.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), null, It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new UpdateResponse { ok = true }));

            var slackAdapter = new SlackAdapter(slackApi.Object);

            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Id = "MockActivityId";
            activity.Object.Conversation = new ConversationAccount
            {
                Id = "MockConversationId",
            };
            activity.Object.Text = "Hello, Bot!";

            var turnContext = new TurnContext(slackAdapter, activity.Object);

            var response = await slackAdapter.UpdateActivityAsync(turnContext, activity.Object, default);

            Assert.Equal(activity.Object.Id, response.Id);
        }

        [Fact]
        public async Task DeleteActivityAsyncShouldFailWithNullReference()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new Mock<SlackClientWrapper>(options.Object);

            // TODO: delete when LoginWithSlack method gets removed from SlackAdapter.
            slackApi.Setup(x => x.TestAuthAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult("mockedUserId"));

            var slackAdapter = new SlackAdapter(slackApi.Object);

            var context = new TurnContext(slackAdapter, new Activity());

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await slackAdapter.DeleteActivityAsync(context, null, default);
            });
        }

        [Fact]
        public async Task DeleteActivityAsyncShouldFailWithNullTurnContext()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new Mock<SlackClientWrapper>(options.Object);

            // TODO: delete when LoginWithSlack method gets removed from SlackAdapter.
            slackApi.Setup(x => x.TestAuthAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult("mockedUserId"));

            var slackAdapter = new SlackAdapter(slackApi.Object);

            var reference = new ConversationReference();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await slackAdapter.DeleteActivityAsync(null, reference, default);
            });
        }

        [Fact]
        public async Task DeleteActivityAsyncShouldFailWithNullChannelId()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new Mock<SlackClientWrapper>(options.Object);

            // TODO: delete when LoginWithSlack method gets removed from SlackAdapter.
            slackApi.Setup(x => x.TestAuthAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult("mockedUserId"));

            var slackAdapter = new SlackAdapter(slackApi.Object);

            var context = new TurnContext(slackAdapter, new Activity());

            var reference = new ConversationReference
            {
                ChannelId = null,
            };

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await slackAdapter.DeleteActivityAsync(context, reference, default);
            });
        }

        [Fact]
        public async Task DeleteActivityAsyncShouldFailWithNullTimestamp()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new Mock<SlackClientWrapper>(options.Object);

            // TODO: delete when LoginWithSlack method gets removed from SlackAdapter.
            slackApi.Setup(x => x.TestAuthAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult("mockedUserId"));

            var slackAdapter = new SlackAdapter(slackApi.Object);

            var context = new TurnContext(slackAdapter, new Activity());

            var reference = new ConversationReference
            {
                ChannelId = "testChannelId",
            };

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await slackAdapter.DeleteActivityAsync(context, reference, default);
            });
        }

        [Fact]
        public async Task DeleteActivityAsyncShouldSucceed()
        {
            var deletedMessages = 0;

            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new Mock<SlackClientWrapper>(options.Object);

            // TODO: delete when LoginWithSlack method gets removed from SlackAdapter.
            slackApi.Setup(x => x.TestAuthAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult("mockedUserId"));
            slackApi.Setup(x => x.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Callback(() => { deletedMessages++; });

            var slackAdapter = new SlackAdapter(slackApi.Object);

            var activity = new Mock<Activity>();
            activity.Object.Timestamp = new DateTimeOffset();

            var context = new TurnContext(slackAdapter, activity.Object);

            var reference = new ConversationReference
            {
                ChannelId = "channelId",
            };

            await slackAdapter.DeleteActivityAsync(context, reference, default);

            Assert.Equal(1, deletedMessages);
        }

        [Fact]
        public async Task SendActivitiesAsyncShouldThrowExceptionWithNullContext()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new SlackClientWrapper(options.Object);

            var slackAdapter = new SlackAdapter(slackApi);

            var activity = new Activity
            {
                Id = "testId",
                Type = ActivityTypes.Message,
                Text = "text",
                Conversation = new ConversationAccount()
                {
                    Id = "testConversationId",
                },
            };

            Activity[] activities = { activity };

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await slackAdapter.SendActivitiesAsync(null, activities, default); });
        }

        [Fact]
        public async Task SendActivitiesAsyncShouldThrowExceptionWithNullActivity()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackApi = new SlackClientWrapper(options.Object);

            var slackAdapter = new SlackAdapter(slackApi);

            var activity = new Activity
            {
                Id = "testId",
                Type = ActivityTypes.Message,
                Text = "text",
                Conversation = new ConversationAccount()
                {
                    Id = "testConversationId",
                },
            };

            var turnContext = new TurnContext(slackAdapter, activity);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await slackAdapter.SendActivitiesAsync(turnContext, null, default); });
        }

        [Fact]
        public async Task SendActivitiesAsyncShouldSucceed()
        {
            var options = new Mock<SlackAdapterOptions>();
            options.Object.VerificationToken = "VerificationToken";
            options.Object.ClientSigningSecret = "ClientSigningSecret";
            options.Object.BotToken = "BotToken";

            var slackResponse = new SlackResponse
            {
                Ok = true,
                TS = "mockedTS",
            };

            var slackApi = new Mock<SlackClientWrapper>(options.Object);
            slackApi.Setup(x => x.PostMessageAsync(It.IsAny<NewSlackMessage>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(slackResponse));

            var slackAdapter = new SlackAdapter(slackApi.Object);

            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "text",
                Conversation = new ConversationAccount()
                {
                    Id = "testConversationId",
                },
            };

            Activity[] activities = { activity };

            var turnContext = new TurnContext(slackAdapter, activity);

            var responses = await slackAdapter.SendActivitiesAsync(turnContext, activities, default);

            Assert.Equal(slackResponse.TS, responses[0].Id);
        }
    }
}
