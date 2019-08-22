using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Webex.Tests
{
    public class WebexAdapterTests
    {
        [Fact]
        public void Constructor_Should_Fail_With_Null_Config()
        {
            Assert.Throws<ArgumentNullException>(() => { new WebexAdapter(null, new Mock<IWebexClient>().Object); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_AccessToken()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = null;
            options.Object.PublicAddress = "Test";

            Assert.Throws<Exception>(() => { new WebexAdapter(options.Object, new Mock<IWebexClient>().Object); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_PublicAddress()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = null;

            Assert.Throws<Exception>(() => { new WebexAdapter(options.Object, new Mock<IWebexClient>().Object); });
        }

        [Fact]
        public void Constructor_WithArguments_Should_Succeed()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            Assert.NotNull(new WebexAdapter(options.Object, new Mock<IWebexClient>().Object));
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Fail_With_Null_ConversationReference()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object, new Mock<IWebexClient>().Object);

            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await webexAdapter.ContinueConversationAsync(null, BotsLogic, default); });
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Fail_With_Null_Logic()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object, new Mock<IWebexClient>().Object);
            var conversationReference = new ConversationReference();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await webexAdapter.ContinueConversationAsync(conversationReference, null, default); });
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Succeed()
        {
            bool callbackInvoked = false;
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object, new Mock<IWebexClient>().Object);
            var conversationReference = new ConversationReference();
            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                callbackInvoked = true;
                return Task.CompletedTask;
            }

            await webexAdapter.ContinueConversationAsync(conversationReference, BotsLogic, default);
            Assert.True(callbackInvoked);
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_HttpRequest()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object, new Mock<IWebexClient>().Object);
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await webexAdapter.ProcessAsync(null, httpResponse.Object, bot.Object, default(CancellationToken));
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_HttpResponse()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object, new Mock<IWebexClient>().Object);
            var httpRequest = new Mock<HttpRequest>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await webexAdapter.ProcessAsync(httpRequest.Object, null, default(IBot), default(CancellationToken));
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_Bot()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object, new Mock<IWebexClient>().Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await webexAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, null, default(CancellationToken));
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Succeed()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object, new Mock<IWebexClient>().Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();
            httpResponse
                .Setup(e => e.WriteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await webexAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default(CancellationToken));
        }

        [Fact]
        public async void UpdateActivityAsync_Should_Throw_NotSupportedException()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object, new Mock<IWebexClient>().Object);

            var activity = new Activity
            {
                Type = ActivityTypes.Event,
            };

            Activity[] activities = { activity };

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await webexAdapter.SendActivitiesAsync(new TurnContext(webexAdapter, activity), activities, default(CancellationToken));
            });
        }

        [Fact]
        public async void SendActivitiesAsync_Should_Fail_With_ActivityType_Not_Message()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object, new Mock<IWebexClient>().Object);
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
            };

            Activity[] activities = { activity };

            var turnContext = new TurnContext(webexAdapter, activity);

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await webexAdapter.SendActivitiesAsync(turnContext, activities, default(CancellationToken));
            });
        }

        [Fact]
        public async void SendActivitiesAsync_Null_toPersonEmail_Should_Succeed()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            // Setup mocked Webex API client
            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<IWebexClient>();
            webexApi.Setup(x => x.CreateMessageAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(expectedResponseId));

            // Create a new Webex Adapter with the mocked classes and get the responses
            var webexAdapter = new WebexAdapter(options.Object, webexApi.Object);

            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = "message";
            activity.Object.Recipient = new ChannelAccount(id: "MockId");
            activity.Object.Text = "Hello, Bot!";

            var turnContext = new TurnContext(webexAdapter, activity.Object);

            var resourceResponse = await webexAdapter.SendActivitiesAsync(turnContext, new Activity[] { activity.Object }, default).ConfigureAwait(false);

            // Assert the result
            Assert.True(resourceResponse[0].Id == expectedResponseId);
        }

        [Fact]
        public async void SendActivitiesAsync_Should_Fail_With_Null_toPersonEmail()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            // Setup mocked Webex API client
            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<IWebexClient>();
            webexApi.Setup(x => x.CreateMessageAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(expectedResponseId));

            // Create a new Webex Adapter with the mocked classes and get the responses
            var webexAdapter = new WebexAdapter(options.Object, webexApi.Object);
            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = "message";
            activity.Object.Text = "Hello, Bot!";

            var turnContext = new TurnContext(webexAdapter, activity.Object);

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await webexAdapter.SendActivitiesAsync(turnContext, new Activity[] { activity.Object }, default(CancellationToken));
            });
        }

        [Fact]
        public async void DeleteActivityAsync_With_ActivityId_Should_Succeed()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexApi = new Mock<IWebexClient>();
            webexApi.Setup(x => x.DeleteMessageAsync(It.IsAny<string>()));

            var webexAdapter = new WebexAdapter(options.Object, webexApi.Object);

            var activity = new Activity();

            var turnContext = new TurnContext(webexAdapter, activity);
            var conversationReference = new ConversationReference
            {
                ActivityId = "MockId",
            };

            await webexAdapter.DeleteActivityAsync(turnContext, conversationReference, default);
        }
    }
}
