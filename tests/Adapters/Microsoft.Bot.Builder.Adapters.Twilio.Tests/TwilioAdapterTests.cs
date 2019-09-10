// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Moq;
using Twilio.Rest.Api.V2010.Account;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Twilio.Tests
{
    public class TwilioAdapterTests
    {
        [Fact]
        public void Constructor_Should_Fail_With_Null_Options()
        {
            Assert.Throws<ArgumentNullException>(() => { new TwilioAdapter(null, new Mock<TwilioClientWrapper>().Object); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_TwilioNumber()
        {
            var options = new TwilioAdapterOptions(null, "Test", "Test", "Test");

            Assert.Throws<ArgumentException>(() => { new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_AccountSid()
        {
            var options = new TwilioAdapterOptions("Test", null, "Test", "Test");

            Assert.Throws<ArgumentException>(() => { new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_AuthToken()
        {
            var options = new TwilioAdapterOptions("Test", "Test", null, "Test");

            Assert.Throws<ArgumentException>(() => { new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_TwilioApi()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            Assert.Throws<ArgumentNullException>(() => { new TwilioAdapter(options, null); });
        }

        [Fact]
        public void Constructor_WithArguments_Succeeds()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            Assert.NotNull(new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object));
        }

        [Fact]
        public async void SendActivitiesAsync_Should_Fail_With_ActivityType_Not_Message()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            var twilioAdapter = new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object);

            var activity = new Activity()
            {
                Type = ActivityTypes.Event,
            };

            Activity[] activities = { activity };

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await twilioAdapter.SendActivitiesAsync(new TurnContext(twilioAdapter, activity), activities, default);
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_HttpRequest()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            var twilioAdapter = new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object);
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await twilioAdapter.ProcessAsync(null, httpResponse.Object, bot.Object, default(CancellationToken));
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_HttpResponse()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            var twilioAdapter = new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object);
            var httpRequest = new Mock<HttpRequest>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await twilioAdapter.ProcessAsync(httpRequest.Object, null, default(IBot), default(CancellationToken));
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_Bot()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            var twilioAdapter = new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await twilioAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, null, default(CancellationToken));
            });
        }

        [Fact(Skip = "Can't mock extension methods")]
        public async void ProcessAsync_Should_Succeed_With_HttpBody()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            var twilioAdapter = new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();
            httpResponse
                .Setup(e => e.WriteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await twilioAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, null, default(CancellationToken));
        }

        [Fact(Skip = "Can't mock extension methods")]
        public async void ProcessAsync_Should_Succeed_With_Null_HttpBody()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            var twilioAdapter = new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();
            httpResponse
                .Setup(e => e.WriteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await twilioAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, null, default(CancellationToken));
        }

        [Fact]
        public async void UpdateActivityAsync_Should_Throw_NotSupportedException()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            var twilioAdapter = new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object);
            var activity = new Activity();
            var turnContext = new TurnContext(twilioAdapter, activity);

            await Assert.ThrowsAsync<NotSupportedException>(async () => { await twilioAdapter.UpdateActivityAsync(turnContext, activity, default); });
        }

        [Fact]
        public async void DeleteActivityAsync_Should_Throw_NotSupportedException()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            var twilioAdapter = new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object);
            var activity = new Activity();
            var turnContext = new TurnContext(twilioAdapter, activity);
            var conversationReference = new ConversationReference();

            await Assert.ThrowsAsync<NotSupportedException>(async () => { await twilioAdapter.DeleteActivityAsync(turnContext, conversationReference, default); });
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Fail_With_Null_ConversationReference()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            var twilioAdapter = new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object);

            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await twilioAdapter.ContinueConversationAsync(null, BotsLogic, default); });
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Fail_With_Null_Logic()
        {
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            var twilioAdapter = new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object);
            var conversationReference = new ConversationReference();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await twilioAdapter.ContinueConversationAsync(conversationReference, null, default); });
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Succeed()
        {
            bool callbackInvoked = false;
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            var twilioAdapter = new TwilioAdapter(options, new Mock<TwilioClientWrapper>().Object);
            var conversationReference = new ConversationReference();

            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                callbackInvoked = true;
                return Task.CompletedTask;
            }

            await twilioAdapter.ContinueConversationAsync(conversationReference, BotsLogic, default);
            Assert.True(callbackInvoked);
        }

        [Fact]
        public async void SendActivitiesAsync_Should_Succeed()
        {
            // Setup mocked ITwilioAdapterOptions
            var options = new TwilioAdapterOptions("Test", "Test", "Test", "Test");

            // Setup mocked Activity and get the message option
            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = "message";
            activity.Object.Attachments = new List<Attachment> { new Attachment(contentUrl: "http://example.com") };
            activity.Object.Conversation = new ConversationAccount(id: "MockId");
            activity.Object.Text = "Hello, Bot!";
            var messageOption = TwilioHelper.ActivityToTwilio(activity.Object, "123456789");

            // Setup mocked Twilio API client
            const string resourceIdentifier = "Mocked Resource Identifier";
            var twilioApi = new Mock<TwilioClientWrapper>();
            twilioApi.Setup(x => x.SendMessage(It.IsAny<CreateMessageOptions>())).Returns(Task.FromResult(resourceIdentifier));

            // Create a new Twilio Adapter with the mocked classes and get the responses
            var twilioAdapter = new TwilioAdapter(options, twilioApi.Object);
            var resourceResponses = await twilioAdapter.SendActivitiesAsync(null, new Activity[] { activity.Object }, default).ConfigureAwait(false);

            // Assert the result
            Assert.True(resourceResponses[0].Id == resourceIdentifier);
        }
    }
}
