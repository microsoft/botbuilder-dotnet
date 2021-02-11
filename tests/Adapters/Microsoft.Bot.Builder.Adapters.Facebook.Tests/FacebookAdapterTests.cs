// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Facebook.Tests
{
    public class FacebookAdapterTests
    {
        private readonly FacebookClientWrapperOptions _testOptions = new FacebookClientWrapperOptions("Test", "Test", "Test");
        private readonly FacebookAdapterOptions _adapterOptions = new FacebookAdapterOptions() { VerifyIncomingRequests = false };

        [Fact]
        public void ConstructorWithArgumentsShouldSucceed()
        {
            Assert.NotNull(new FacebookAdapter(new FacebookClientWrapper(_testOptions), _adapterOptions));
        }

        [Fact]
        public void ConstructorShouldFailWithNullClient()
        {
            Assert.Throws<ArgumentNullException>(() => { new FacebookAdapter((FacebookClientWrapper)null, _adapterOptions); });
        }

        [Fact]
        public async void ContinueConversationAsyncShouldSucceed()
        {
            var callbackInvoked = false;
            var facebookAdapter = new FacebookAdapter(new FacebookClientWrapper(_testOptions), _adapterOptions);
            var conversationReference = new ConversationReference();
            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                callbackInvoked = true;
                return Task.CompletedTask;
            }

            await facebookAdapter.ContinueConversationAsync(conversationReference, BotsLogic, default);
            Assert.True(callbackInvoked);
        }

        [Fact]
        public async void ContinueConversationAsyncShouldFailWithNullConversationReference()
        {
            var facebookAdapter = new FacebookAdapter(new FacebookClientWrapper(_testOptions), _adapterOptions);
            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookAdapter.ContinueConversationAsync(null, BotsLogic, default); });
        }

        [Fact]
        public async void ContinueConversationAsyncShouldFailWithNullLogic()
        {
            var facebookAdapter = new FacebookAdapter(new FacebookClientWrapper(_testOptions), _adapterOptions);
            var conversationReference = new ConversationReference();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await facebookAdapter.ContinueConversationAsync(conversationReference, null, default); });
        }

        [Fact]
        public async Task DeleteActivityAsyncShouldThrowNotImplementedException()
        {
            var facebookAdapter = new FacebookAdapter(new FacebookClientWrapper(_testOptions), _adapterOptions);
            var activity = new Activity();
            var conversationReference = new ConversationReference();
            using (var turnContext = new TurnContext(facebookAdapter, activity))
            {
                await Assert.ThrowsAsync<NotImplementedException>(() => facebookAdapter.DeleteActivityAsync(turnContext, conversationReference, default));
            }
        }

        [Fact]
        public async void ProcessAsyncShouldSucceedWithCorrectData()
        {
            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/Payload.json");
            var facebookClientWrapper = new Mock<FacebookClientWrapper>(_testOptions);
            var facebookAdapter = new FacebookAdapter(facebookClientWrapper.Object, _adapterOptions);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            httpRequest.SetupGet(req => req.Query[It.IsAny<string>()]).Returns("test");
            httpRequest.SetupGet(req => req.Body).Returns(stream);

            await facebookAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default(CancellationToken));

            bot.Verify(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async void ProcessAsyncShouldSucceedWithStandbyMessages()
        {
            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/PayloadWithStandby.json");
            var facebookClientWrapper = new Mock<FacebookClientWrapper>(_testOptions);
            var facebookAdapter = new FacebookAdapter(facebookClientWrapper.Object, _adapterOptions);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            httpRequest.SetupGet(req => req.Query[It.IsAny<string>()]).Returns("test");
            httpRequest.SetupGet(req => req.Body).Returns(stream);

            await facebookAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default(CancellationToken));
            bot.Verify(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async void ProcessAsyncShouldVerifyWebhookOnHubModeSubscribe()
        {
            var testOptionsVerifyEnabled = new FacebookAdapterOptions()
            {
                VerifyIncomingRequests = true
            };

            var facebookClientWrapper = new Mock<FacebookClientWrapper>(_testOptions);
            var facebookAdapter = new FacebookAdapter(facebookClientWrapper.Object, testOptionsVerifyEnabled);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            facebookClientWrapper.Setup(api => api.VerifyWebhookAsync(It.IsAny<HttpRequest>(), It.IsAny<HttpResponse>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            httpRequest.SetupGet(req => req.Query[It.IsAny<string>()]).Returns("subscribe");

            await facebookAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default(CancellationToken));
            bot.Verify(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProcessAsyncShouldThrowExceptionWithUnverifiedSignature()
        {
            var testOptionsVerifyEnabled = new FacebookAdapterOptions()
            {
                VerifyIncomingRequests = true
            };

            var facebookClientWrapper = new Mock<FacebookClientWrapper>(_testOptions);
            var facebookAdapter = new FacebookAdapter(facebookClientWrapper.Object, testOptionsVerifyEnabled);
            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/Payload.json");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            httpRequest.SetupGet(req => req.Query[It.IsAny<string>()]).Returns("test");
            httpRequest.SetupGet(req => req.Body).Returns(stream);

            httpResponse.Setup(_ => _.Body.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback((byte[] data, int offset, int length, CancellationToken token) =>
                {
                    if (length > 0)
                    {
                        var actual = Encoding.UTF8.GetString(data);
                    }
                });

            await Assert.ThrowsAsync<AuthenticationException>(() => facebookAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default(CancellationToken)));
        }

        [Fact]
        public async void SendActivitiesAsyncShouldSucceedWithActivityTypeMessage()
        {
            const string testResponse = "Test Response";
            var facebookClientWrapper = new Mock<FacebookClientWrapper>(_testOptions);
            var facebookAdapter = new FacebookAdapter(facebookClientWrapper.Object, _adapterOptions);
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "Test text",
                Conversation = new ConversationAccount()
                {
                    Id = "Test id",
                },
                ChannelData = new FacebookMessage("recipientId", new Message(), "messagingtype"),
            };
            Activity[] activities = { activity };
            ResourceResponse[] responses = null;

            facebookClientWrapper.Setup(api => api.SendMessageAsync(It.IsAny<string>(), It.IsAny<FacebookMessage>(), It.IsAny<HttpMethod>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(testResponse));

            using (var turnContext = new TurnContext(facebookAdapter, activity))
            {
                responses = await facebookAdapter.SendActivitiesAsync(turnContext, activities, default);
            }

            Assert.Equal(testResponse, responses[0].Id);
        }

        [Fact]
        public async void SendActivitiesAsyncShouldSucceedWithActivityTypeMessageAndAttachments()
        {
            const string testResponse = "Test Response";
            var facebookClientWrapper = new Mock<FacebookClientWrapper>(_testOptions);
            var facebookAdapter = new FacebookAdapter(facebookClientWrapper.Object, _adapterOptions);
            var attachments = new List<Attachment>();
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "Test text",
                Conversation = new ConversationAccount()
                {
                    Id = "Test id",
                },
                ChannelData = new FacebookMessage("recipientId", new Message(), "messagingtype"),
                Attachments = attachments,
            };
            Activity[] activities = { activity };
            ResourceResponse[] responses = null;

            attachments.Add(new Attachment("text/html", "http://contoso.com"));
            facebookClientWrapper.Setup(api => api.SendMessageAsync(It.IsAny<string>(), It.IsAny<FacebookMessage>(), It.IsAny<HttpMethod>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(testResponse));

            using (var turnContext = new TurnContext(facebookAdapter, activity))
            {
                responses = await facebookAdapter.SendActivitiesAsync(turnContext, activities, default);
            }

            Assert.Equal(testResponse, responses[0].Id);
        }

        [Fact]
        public async void SendActivitiesAsyncShouldSucceedAndNoActivityReturnedWithActivityTypeNotMessage()
        {
            const string testResponse = "Test Response";
            var facebookClientWrapper = new Mock<FacebookClientWrapper>(_testOptions);
            var facebookAdapter = new FacebookAdapter(facebookClientWrapper.Object, _adapterOptions);
            var attachments = new List<Attachment>();
            var activity = new Activity
            {
                Type = ActivityTypes.Trace,
                Text = "Test text",
                Conversation = new ConversationAccount()
                {
                    Id = "Test id",
                },
                ChannelData = new FacebookMessage("recipientId", new Message(), "messagingtype"),
                Attachments = attachments,
            };
            Activity[] activities = { activity };
            ResourceResponse[] responses = null;

            attachments.Add(new Attachment("text/html", "http://contoso.com"));
            facebookClientWrapper.Setup(api => api.SendMessageAsync(It.IsAny<string>(), It.IsAny<FacebookMessage>(), It.IsAny<HttpMethod>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(testResponse));

            using (var turnContext = new TurnContext(facebookAdapter, activity))
            {
                responses = await facebookAdapter.SendActivitiesAsync(turnContext, activities, default);
            }

            Assert.True(responses.Length == 0);
        }

        [Fact]
        public async void SendActivitiesAsyncShouldPostToFacebookOnPassThreadControl()
        {
            const string testResponse = "Test Response";
            var facebookClientWrapper = new Mock<FacebookClientWrapper>(_testOptions);
            var facebookAdapter = new FacebookAdapter(facebookClientWrapper.Object, _adapterOptions);
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
                Text = "Test text",
                Name = "pass_thread_control",
                Conversation = new ConversationAccount()
                {
                    Id = "Test id",
                },
                ChannelData = new FacebookMessage("recipientId", new Message(), "messagingtype"),
            };
            Activity[] activities = { activity };
            ResourceResponse[] responses = null;

            facebookClientWrapper.Setup(api => api.SendMessageAsync(It.IsAny<string>(), It.IsAny<FacebookMessage>(), It.IsAny<HttpMethod>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(testResponse));
            facebookClientWrapper.Setup(api => api.PassThreadControlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.FromResult(true));

            using (var turnContext = new TurnContext(facebookAdapter, activity))
            {
                responses = await facebookAdapter.SendActivitiesAsync(turnContext, activities, default);
            }

            Assert.Equal(testResponse, responses[0].Id);
            facebookClientWrapper.Verify(api => api.PassThreadControlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
        }

        [Fact]
        public async void SendActivitiesAsyncShouldPostToFacebookOnTakeThreadControl()
        {
            const string testResponse = "Test Response";
            var facebookClientWrapper = new Mock<FacebookClientWrapper>(_testOptions);
            var facebookAdapter = new FacebookAdapter(facebookClientWrapper.Object, _adapterOptions);
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
                Text = "Test text",
                Name = "take_thread_control",
                Conversation = new ConversationAccount()
                {
                    Id = "Test id",
                },
                ChannelData = new FacebookMessage("recipientId", new Message(), "messagingtype"),
            };
            Activity[] activities = { activity };
            ResourceResponse[] responses = null;

            facebookClientWrapper.Setup(api => api.SendMessageAsync(It.IsAny<string>(), It.IsAny<FacebookMessage>(), It.IsAny<HttpMethod>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(testResponse));
            facebookClientWrapper.Setup(api => api.TakeThreadControlAsync(It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.FromResult(true));

            using (var turnContext = new TurnContext(facebookAdapter, activity))
            {
                responses = await facebookAdapter.SendActivitiesAsync(turnContext, activities, default);
            }

            Assert.Equal(testResponse, responses[0].Id);
            facebookClientWrapper.Verify(api => api.TakeThreadControlAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
        }

        [Fact]
        public async void SendActivitiesAsyncShouldPostToFacebookOnRequestThreadControl()
        {
            const string testResponse = "Test Response";
            var facebookClientWrapper = new Mock<FacebookClientWrapper>(_testOptions);
            var facebookAdapter = new FacebookAdapter(facebookClientWrapper.Object, _adapterOptions);
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
                Text = "Test text",
                Name = "request_thread_control",
                Conversation = new ConversationAccount()
                {
                    Id = "Test id",
                },
                ChannelData = new FacebookMessage("recipientId", new Message(), "messagingtype"),
            };
            Activity[] activities = { activity };
            ResourceResponse[] responses = null;

            facebookClientWrapper.Setup(api => api.SendMessageAsync(It.IsAny<string>(), It.IsAny<FacebookMessage>(), It.IsAny<HttpMethod>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(testResponse));
            facebookClientWrapper.Setup(api => api.RequestThreadControlAsync(It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.FromResult(true));

            using (var turnContext = new TurnContext(facebookAdapter, activity))
            {
                responses = await facebookAdapter.SendActivitiesAsync(turnContext, activities, default);
            }

            Assert.Equal(testResponse, responses[0].Id);
            facebookClientWrapper.Verify(api => api.RequestThreadControlAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
        }
        
        [Fact]
        public async Task UpdateActivityAsyncShouldThrowNotImplementedException()
        {
            var facebookClientWrapper = new Mock<FacebookClientWrapper>(_testOptions);
            var facebookAdapter = new FacebookAdapter(facebookClientWrapper.Object, _adapterOptions);
            var activity = new Activity();

            using (var turnContext = new TurnContext(facebookAdapter, activity))
            {
                await Assert.ThrowsAsync<NotImplementedException>(() => facebookAdapter.UpdateActivityAsync(turnContext, activity, default));
            }
        }
    }
}
