// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Skills.Tests
{
    public class ChannelApiMiddlewareTests
    {
        [Fact]
        public async Task DoesNotInterceptRegularActivities()
        {
            var sut = new ChannelApiMiddleware();
            var mockTurnContext = new Mock<ITurnContext>();
            mockTurnContext.Setup(x => x.Activity).Returns(MessageFactory.Text("A test message activity"));

            var nextCalled = false;
            await sut.OnTurnAsync(mockTurnContext.Object, cancellationToken =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

            // Assert that next was called (means the activity was sent to the bot)
            Assert.True(nextCalled);
        }

        [Theory]
        [InlineData(ChannelApiMethods.SendToConversation, null)]
        [InlineData(ChannelApiMethods.ReplyToActivity, "someGuid")]
        public async Task SendsEndOfConversationToTheBot(string channelApiMethod, string replyToId)
        {
            var sut = new ChannelApiMiddleware();
            var mockAdapter = new Mock<BotFrameworkAdapter>(new Mock<ICredentialProvider>().Object, null, null, null, null, NullLogger.Instance);
            var eocActivity = (Activity)Activity.CreateEndOfConversationActivity();
            eocActivity.ReplyToId = replyToId;
            eocActivity.Text = "some text";
            eocActivity.Code = "some code";
            eocActivity.Value = "some value";
            eocActivity.Entities = new List<Entity> { new Entity("myEntity") };
            eocActivity.LocalTimestamp = DateTimeOffset.Now.ToLocalTime();
            eocActivity.Timestamp = DateTimeOffset.Now.ToUniversalTime();
            eocActivity.ChannelData = "some channel data";
            eocActivity.Properties = new JObject(new JProperty("someProperty", "some content"));

            var skillInvokeActivity = CreateSkillInvokeActivity(channelApiMethod, eocActivity);
            var args = (ChannelApiArgs)skillInvokeActivity.Value;
            var testTurnContext = new TurnContext(mockAdapter.Object, skillInvokeActivity);

            var nextCalled = false;
            await sut.OnTurnAsync(testTurnContext, cancellationToken =>
            {
                // Next should be called with turnContext set to the activity payload. 
                nextCalled = true;
                return Task.CompletedTask;
            });

            // Assert that next was called (means the activity was sent to the bot)
            Assert.True(nextCalled);

            // Assert that the eoc activity and properties were sent to the bot.
            Assert.Equal(ActivityTypes.EndOfConversation, testTurnContext.Activity.Type);
            Assert.Equal(eocActivity.Type, testTurnContext.Activity.Type);
            Assert.Equal(eocActivity.Id, testTurnContext.Activity.Id);
            Assert.Equal(eocActivity.ReplyToId, testTurnContext.Activity.ReplyToId);
            Assert.Equal(eocActivity.Text, testTurnContext.Activity.Text);
            Assert.Equal(eocActivity.Code, testTurnContext.Activity.Code);
            Assert.Equal(eocActivity.Entities, testTurnContext.Activity.Entities);
            Assert.Equal(eocActivity.LocalTimestamp, testTurnContext.Activity.LocalTimestamp);
            Assert.Equal(eocActivity.Timestamp, testTurnContext.Activity.Timestamp);
            Assert.Equal(eocActivity.Value, testTurnContext.Activity.Value);
            Assert.Equal(eocActivity.ChannelData, testTurnContext.Activity.ChannelData);
            Assert.Equal(eocActivity.Properties, testTurnContext.Activity.Properties);

            // Assert args property
            Assert.Null(args.Exception);
            var response = args.Result as ResourceResponse;
            Assert.NotNull(response);
            Assert.False(string.IsNullOrWhiteSpace(response.Id));
        }

        [Theory]
        [InlineData(ChannelApiMethods.SendToConversation, null)]
        [InlineData(ChannelApiMethods.ReplyToActivity, "someGuid")]
        public async Task SendsEventActivitiesToTheBot(string channelApiMethod, string replyToId)
        {
            var sut = new ChannelApiMiddleware();
            var mockAdapter = new Mock<BotFrameworkAdapter>(new Mock<ICredentialProvider>().Object, null, null, null, null, NullLogger.Instance);
            var eventActivity = (Activity)Activity.CreateEventActivity();
            eventActivity.ReplyToId = replyToId;
            eventActivity.Name = "EventName";
            eventActivity.Value = "some value";
            eventActivity.RelatesTo = new ConversationReference("123");

            eventActivity.Entities = new List<Entity> { new Entity("myEntity") };
            eventActivity.LocalTimestamp = DateTimeOffset.Now.ToLocalTime();
            eventActivity.Timestamp = DateTimeOffset.Now.ToUniversalTime();
            eventActivity.ChannelData = "some channel data";
            eventActivity.Properties = new JObject(new JProperty("someProperty", "some content"));

            var skillInvokeActivity = CreateSkillInvokeActivity(channelApiMethod, eventActivity);
            var args = (ChannelApiArgs)skillInvokeActivity.Value;
            var testTurnContext = new TurnContext(mockAdapter.Object, skillInvokeActivity);

            var nextCalled = false;
            await sut.OnTurnAsync(testTurnContext, cancellationToken =>
            {
                // Next should be called with turnContext set to the activity payload. 
                nextCalled = true;
                return Task.CompletedTask;
            });

            // Assert that next was called (means the activity was sent to the bot)
            Assert.True(nextCalled);

            // Assert that the eoc activity and properties were sent to the bot.
            Assert.Equal(ActivityTypes.Event, testTurnContext.Activity.Type);
            Assert.Equal(eventActivity.Type, testTurnContext.Activity.Type);
            Assert.Equal(eventActivity.Id, testTurnContext.Activity.Id);
            Assert.Equal(eventActivity.ReplyToId, testTurnContext.Activity.ReplyToId);
            Assert.Equal(eventActivity.Name, testTurnContext.Activity.Name);
            Assert.Equal(eventActivity.RelatesTo.ActivityId, testTurnContext.Activity.RelatesTo.ActivityId);
            Assert.Equal(eventActivity.Value, testTurnContext.Activity.Value);
            Assert.Equal(eventActivity.Entities, testTurnContext.Activity.Entities);
            Assert.Equal(eventActivity.LocalTimestamp, testTurnContext.Activity.LocalTimestamp);
            Assert.Equal(eventActivity.Timestamp, testTurnContext.Activity.Timestamp);
            Assert.Equal(eventActivity.ChannelData, testTurnContext.Activity.ChannelData);
            Assert.Equal(eventActivity.Properties, testTurnContext.Activity.Properties);

            // Assert args property
            Assert.Null(args.Exception);
            var response = args.Result as ResourceResponse;
            Assert.NotNull(response);
            Assert.False(string.IsNullOrWhiteSpace(response.Id));
        }

        [Theory]
        [InlineData(ChannelApiMethods.SendToConversation)]
        [InlineData(ChannelApiMethods.ReplyToActivity)]
        public void SendsAllOtherActivitiesToTheChannel(string channelApiMethod)
        {
            var testActivty = new Activity();
        }

        private Activity CreateSkillInvokeActivity(string channelApiMethod, Activity activityPayload)
        {
            var apiArgs = new ChannelApiArgs
            {
                Method = channelApiMethod,
                Args = string.IsNullOrWhiteSpace(activityPayload.ReplyToId)
                    ? new object[] { activityPayload }
                    : new object[]
                    {
                        activityPayload,
                        activityPayload.ReplyToId
                    }
            };

            var activity = Activity.CreateInvokeActivity();
            activity.Name = SkillHandler.InvokeActivityName;
            activity.Value = apiArgs;

            return (Activity)activity;
        }
    }
}
