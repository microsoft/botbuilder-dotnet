using System;
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
            Assert.Throws<ArgumentNullException>(() => { new WebexAdapter(null); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_AccessToken()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = null;
            options.Object.PublicAddress = "Test";

            Assert.Throws<Exception>(() => { new WebexAdapter(options.Object); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_PublicAddress()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = null;

            Assert.Throws<Exception>(() => { new WebexAdapter(options.Object); });
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Fail_With_Null_ConversationReference()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object);

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

            var webexAdapter = new WebexAdapter(options.Object);
            var conversationReference = new ConversationReference();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await webexAdapter.ContinueConversationAsync(conversationReference, null, default); });
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Succeed()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object);
            var conversationReference = new ConversationReference();
            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            await webexAdapter.ContinueConversationAsync(conversationReference, BotsLogic, default);
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_HttpRequest()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object);
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

            var webexAdapter = new WebexAdapter(options.Object);
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

            var webexAdapter = new WebexAdapter(options.Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await webexAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, null, default(CancellationToken));
            });
        }

        [Fact(Skip = "Can't mock extension methods")]
        public async void ProcessAsync_Should_Succeed()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object);
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

            var webexAdapter = new WebexAdapter(options.Object);
            var activity = new Activity();
            var turnContext = new TurnContext(webexAdapter, activity);

            await Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await webexAdapter.UpdateActivityAsync(turnContext, activity, default(CancellationToken));
            });
        }
    }
}
