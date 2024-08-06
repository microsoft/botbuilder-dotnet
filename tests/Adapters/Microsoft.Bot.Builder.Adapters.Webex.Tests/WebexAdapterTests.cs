// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Thrzn41.WebexTeams.Version1;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Webex.Tests
{
    public class WebexAdapterTests
    {
        private static readonly Uri _testPublicAddress = new Uri("http://contoso.com");
        private readonly Person _identity = JsonConvert.DeserializeObject<Person>(File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"/Files/Person.json")));
        private readonly WebexClientWrapperOptions _testOptions = new WebexClientWrapperOptions("Test", _testPublicAddress, "Test");
        private readonly WebexAdapterOptions _adapterOptions = new WebexAdapterOptions();

        [Fact]
        public void ConstructorWithArgumentsShouldSucceed()
        {
            Assert.NotNull(new WebexAdapter(new Mock<WebexClientWrapper>(_testOptions).Object, _adapterOptions));
        }

        [Fact]
        public void ConstructorShouldFailWithNullClient()
        {
            Assert.Throws<ArgumentNullException>(() => { new WebexAdapter((WebexClientWrapper)null, _adapterOptions); });
        }

        [Fact]
        public async Task ContinueConversationAsyncShouldSucceed()
        {
            var callbackInvoked = false;

            var webexAdapter = new WebexAdapter(new Mock<WebexClientWrapper>(_testOptions).Object, _adapterOptions);
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
        public async Task ContinueConversationAsyncShouldFailWithNullConversationReference()
        {
            var webexAdapter = new WebexAdapter(new Mock<WebexClientWrapper>(_testOptions).Object, _adapterOptions);

            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await webexAdapter.ContinueConversationAsync(null, BotsLogic, default); });
        }

        [Fact]
        public async Task ContinueConversationAsyncShouldFailWithNullLogic()
        {
            var webexAdapter = new WebexAdapter(new Mock<WebexClientWrapper>(_testOptions).Object, _adapterOptions);
            var conversationReference = new ConversationReference();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await webexAdapter.ContinueConversationAsync(conversationReference, null, default); });
        }

        [Fact]
        public async Task ProcessAsyncWithEvenTypeCreatedShouldSucceed()
        {
            var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\Files\Message.json")));
            var payload = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"/Files/Payload.json"));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));
            var call = false;

            var webexApi = new Mock<WebexClientWrapper>(_testOptions);
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMeAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(_identity));
            webexApi.Setup(x => x.ValidateSignature(It.IsAny<HttpRequest>(), It.IsAny<string>())).Returns(true);
            webexApi.Setup(x => x.GetMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(message));

            var webexAdapter = new WebexAdapter(webexApi.Object, _adapterOptions);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupGet(req => req.Body).Returns(stream);

            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();
            bot.Setup(x => x.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>())).Callback(() =>
            {
                call = true;
            });

            await webexAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default);

            Assert.True(call);
        }

        [Fact]
        public async Task ProcessAsyncWithEvenTypeUpdatedShouldSucceed()
        {
            var webexApi = new Mock<WebexClientWrapper>(_testOptions);
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMeAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(_identity));
            webexApi.Setup(x => x.ValidateSignature(It.IsAny<HttpRequest>(), It.IsAny<string>())).Returns(true);

            var webexAdapter = new WebexAdapter(webexApi.Object, _adapterOptions);

            var payload = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"/Files/Payload2.json"));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));
            var call = false;

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupGet(req => req.Body).Returns(stream);

            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();
            bot.Setup(x => x.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>())).Callback(() =>
            {
                call = true;
            });

            await webexAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default);

            Assert.True(call);
        }

        [Fact]
        public async Task ProcessAsyncWithAttachmentActionsShouldSucceed()
        {
            var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"/Files/MessageWithInputs.json")));
            var payload = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"/Files/PayloadAttachmentActions.json"));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));
            var call = false;

            var webexApi = new Mock<WebexClientWrapper>(_testOptions);
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMeAsync(new CancellationToken())).Returns(Task.FromResult(_identity));
            webexApi.Setup(x => x.ValidateSignature(It.IsAny<HttpRequest>(), It.IsAny<string>())).Returns(true);
            webexApi.Setup(x => x.GetAttachmentActionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(message));

            var webexAdapter = new WebexAdapter(webexApi.Object, _adapterOptions);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupGet(req => req.Body).Returns(stream);

            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();
            bot.Setup(x => x.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>())).Callback(() =>
            {
                call = true;
            });

            await webexAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, new CancellationToken());

            Assert.True(call);
        }

        [Fact]
        public async Task ProcessAsyncShouldFailWithNullHttpRequest()
        {
            var webexAdapter = new WebexAdapter(new Mock<WebexClientWrapper>(_testOptions).Object, _adapterOptions);
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await webexAdapter.ProcessAsync(null, httpResponse.Object, bot.Object, default);
            });
        }

        [Fact]
        public async Task ProcessAsyncShouldFailWithNullHttpResponse()
        {
            var webexAdapter = new WebexAdapter(new Mock<WebexClientWrapper>(_testOptions).Object, _adapterOptions);
            var httpRequest = new Mock<HttpRequest>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await webexAdapter.ProcessAsync(httpRequest.Object, null, default, default);
            });
        }

        [Fact]
        public async Task ProcessAsyncShouldFailWithNullBot()
        {
            var webexAdapter = new WebexAdapter(new Mock<WebexClientWrapper>(_testOptions).Object, _adapterOptions);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await webexAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, null, default);
            });
        }

        [Fact]
        public async Task ProcessAsyncShouldFailWithNonMatchingSignature()
        {
            var webexApi = new Mock<WebexClientWrapper>(_testOptions);
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMeAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(_identity));

            var webexAdapter = new WebexAdapter(webexApi.Object, _adapterOptions);

            var payload = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"/Files/Payload2.json"));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupGet(req => req.Body).Returns(stream);
            httpRequest.Setup(req => req.Headers.ContainsKey(It.IsAny<string>())).Returns(true);
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns("wrong_signature");

            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<AuthenticationException>(async () =>
            {
                await webexAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default);
            });
        }

        [Fact]
        public async Task UpdateActivityAsyncShouldThrowNotSupportedException()
        {
            var webexAdapter = new WebexAdapter(new Mock<WebexClientWrapper>(_testOptions).Object, _adapterOptions);

            var activity = new Activity();

            var turnContext = new TurnContext(webexAdapter, activity);

            await Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await webexAdapter.UpdateActivityAsync(turnContext, activity, default);
            });
        }

        [Fact]
        public async Task SendActivitiesAsyncNotNullToPersonEmailShouldSucceed()
        {
            // Setup mocked Webex API client
            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<WebexClientWrapper>(_testOptions);
            webexApi.Setup(x => x.CreateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<Uri>>(), It.IsAny<MessageTextType>(), It.IsAny<MessageTarget>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResponseId));

            // Create a new Webex Adapter with the mocked classes and get the responses
            var webexAdapter = new WebexAdapter(webexApi.Object, _adapterOptions);

            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = "message";
            activity.Object.Recipient = new ChannelAccount(id: "MockId");
            activity.Object.Text = "Hello, Bot!";

            var turnContext = new TurnContext(webexAdapter, activity.Object);

            var resourceResponse = await webexAdapter.SendActivitiesAsync(turnContext, new Activity[] { activity.Object }, default);

            // Assert the result
            Assert.True(resourceResponse[0].Id == expectedResponseId);
        }

        [Fact]
        public async Task SendActivitiesAsyncWithAttachmentShouldSucceed()
        {
            // Setup mocked Webex API client
            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<WebexClientWrapper>(_testOptions);
            webexApi.Setup(x => x.CreateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<Uri>>(), It.IsAny<MessageTextType>(), It.IsAny<MessageTarget>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResponseId));

            // Create a new Webex Adapter with the mocked classes and get the responses
            var webexAdapter = new WebexAdapter(webexApi.Object, _adapterOptions);

            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = "message";
            activity.Object.Recipient = new ChannelAccount(id: "MockId");
            activity.Object.Text = "Hello, Bot!";
            activity.Object.Attachments = new List<Schema.Attachment>
            {
                new Schema.Attachment("image/png", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU"),
            };

            var turnContext = new TurnContext(webexAdapter, activity.Object);

            var resourceResponse = await webexAdapter.SendActivitiesAsync(turnContext, new Activity[] { activity.Object }, default);

            // Assert the result
            Assert.True(resourceResponse[0].Id == expectedResponseId);
        }

        [Fact]
        public async Task SendActivitiesAsyncWithAttachmentActionsShouldSucceed()
        {
            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<WebexClientWrapper>(_testOptions);
            webexApi.Setup(x => x.CreateMessageWithAttachmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<Schema.Attachment>>(), It.IsAny<MessageTextType>(), It.IsAny<MessageTarget>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResponseId));

            var webexAdapter = new WebexAdapter(webexApi.Object, _adapterOptions);

            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = "message";
            activity.Object.Recipient = new ChannelAccount(id: "MockId");
            activity.Object.Text = "Hello, Bot!";
            activity.Object.Attachments = new List<Schema.Attachment>
            {
                new Schema.Attachment("application/vnd.microsoft.card.adaptive"),
            };

            var turnContext = new TurnContext(webexAdapter, activity.Object);

            var resourceResponse = await webexAdapter.SendActivitiesAsync(turnContext, new Activity[] { activity.Object }, default);

            Assert.True(resourceResponse[0].Id == expectedResponseId);
        }

        [Fact]
        public async Task SendActivitiesAsyncShouldSucceedAndNoActivityReturnedWithActivityTypeNotMessage()
        {
            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<WebexClientWrapper>(_testOptions);
            webexApi.Setup(x => x.CreateMessageWithAttachmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<Schema.Attachment>>(), It.IsAny<MessageTextType>(), It.IsAny<MessageTarget>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResponseId));

            var webexAdapter = new WebexAdapter(webexApi.Object, _adapterOptions);

            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = ActivityTypes.Trace;
            activity.Object.Recipient = new ChannelAccount(id: "MockId");
            activity.Object.Text = "Trace content";

            var turnContext = new TurnContext(webexAdapter, activity.Object);

            var resourceResponse = await webexAdapter.SendActivitiesAsync(turnContext, new Activity[] { activity.Object }, default);

            Assert.True(resourceResponse.Length == 0);
        }

        [Fact]
        public async Task SendActivitiesAsyncShouldFailWithNullToPersonEmail()
        {
            // Setup mocked Webex API client
            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<WebexClientWrapper>(_testOptions);
            webexApi.Setup(x => x.CreateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<Uri>>(), It.IsAny<MessageTextType>(), It.IsAny<MessageTarget>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResponseId));

            // Create a new Webex Adapter with the mocked classes and get the responses
            var webexAdapter = new WebexAdapter(webexApi.Object, _adapterOptions);
            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = "message";
            activity.Object.Text = "Hello, Bot!";

            var turnContext = new TurnContext(webexAdapter, activity.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await webexAdapter.SendActivitiesAsync(turnContext, new Activity[] { activity.Object }, default);
            });
        }

        [Fact]
        public async Task DeleteActivityAsyncWithActivityIdShouldSucceed()
        {
            var deletedMessages = 0;

            var webexApi = new Mock<WebexClientWrapper>(_testOptions);
            webexApi.Setup(x => x.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback(() => { deletedMessages++; });

            var webexAdapter = new WebexAdapter(webexApi.Object, _adapterOptions);

            var activity = new Activity();

            var turnContext = new TurnContext(webexAdapter, activity);
            var conversationReference = new ConversationReference
            {
                ActivityId = "MockId",
            };

            await webexAdapter.DeleteActivityAsync(turnContext, conversationReference, default);

            Assert.Equal(1, deletedMessages);
        }
    }
}
