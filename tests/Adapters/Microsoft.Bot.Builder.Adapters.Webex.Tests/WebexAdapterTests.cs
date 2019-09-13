// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly Uri _testPublicAddress = new Uri("http://contoso.com");
        private readonly Person _identity = JsonConvert.DeserializeObject<Person>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Person.json"));

        [Fact]
        public void ConstructorShouldFailWithNullConfig()
        {
            Assert.Throws<ArgumentNullException>(() => { new WebexAdapter(null, new Mock<WebexClientWrapper>().Object); });
        }

        [Fact]
        public void ConstructorShouldFailWithNullAccessToken()
        {
            var options = new WebexAdapterOptions(null, _testPublicAddress, "Test");

            Assert.Throws<Exception>(() => { new WebexAdapter(options, new Mock<WebexClientWrapper>().Object); });
        }

        [Fact]
        public void ConstructorShouldFailWithNullPublicAddress()
        {
            var options = new WebexAdapterOptions("Test", null, "Test");

            Assert.Throws<Exception>(() => { new WebexAdapter(options, new Mock<WebexClientWrapper>().Object); });
        }

        [Fact]
        public void ConstructorWithArgumentsShouldSucceed()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            Assert.NotNull(new WebexAdapter(options, new Mock<WebexClientWrapper>().Object));
        }

        [Fact]
        public async void ContinueConversationAsyncShouldFailWithNullConversationReference()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);

            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await webexAdapter.ContinueConversationAsync(null, BotsLogic, default); });
        }

        [Fact]
        public async void ContinueConversationAsyncShouldFailWithNullLogic()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);
            var conversationReference = new ConversationReference();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await webexAdapter.ContinueConversationAsync(conversationReference, null, default); });
        }

        [Fact]
        public async void ContinueConversationAsyncShouldSucceed()
        {
            var callbackInvoked = false;

            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);
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
        public async void GetIdentityAsyncShouldSucceed()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMeAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(_identity));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            await webexAdapter.GetIdentityAsync(new CancellationToken());

            Assert.Equal(_identity.Id, webexAdapter.Identity.Id);
        }

        [Fact]
        public async void ProcessAsyncShouldFailWithNullHttpRequest()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await webexAdapter.ProcessAsync(null, httpResponse.Object, bot.Object, default);
            });
        }

        [Fact]
        public async void ProcessAsyncShouldFailWithNullHttpResponse()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);
            var httpRequest = new Mock<HttpRequest>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await webexAdapter.ProcessAsync(httpRequest.Object, null, default, default);
            });
        }

        [Fact]
        public async void ProcessAsyncShouldFailWithNullBot()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await webexAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, null, default);
            });
        }

        [Fact]
        public async void ProcessAsyncWithEvenTypeCreatedShouldSucceed()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Message.json"));
            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Payload.json");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));
            var call = false;

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMeAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(_identity));
            webexApi.Setup(x => x.GetMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(message));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupGet(req => req.Body).Returns(stream);
            httpRequest.Setup(req => req.Headers.ContainsKey(It.IsAny<string>())).Returns(true);
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns("61E7F071CE5C9FA21C773E7D6E9C6FF3B8A21F80");

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
        public async void ProcessAsyncWithEvenTypeUpdatedShouldSucceed()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMeAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(_identity));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Payload2.json");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));
            var call = false;

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupGet(req => req.Body).Returns(stream);
            httpRequest.Setup(req => req.Headers.ContainsKey(It.IsAny<string>())).Returns(true);
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns("9C32875928D2901E0BE90AEDDF4063174E25BB4E");

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
        public async void ProcessAsyncShouldFailWithNonMatchingSignature()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMeAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(_identity));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Payload2.json");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupGet(req => req.Body).Returns(stream);
            httpRequest.Setup(req => req.Headers.ContainsKey(It.IsAny<string>())).Returns(true);
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns("wrong_signature");

            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await webexAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default);
            });
        }

        [Fact]
        public async void ProcessAsyncWithAttachmentActionsShouldSucceed()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\MessageWithInputs.json"));
            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\PayloadAttachmentActions.json");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));
            var call = false;

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMeAsync(new CancellationToken())).Returns(Task.FromResult(_identity));
            webexApi.Setup(x => x.GetAttachmentActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(message));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupGet(req => req.Body).Returns(stream);
            httpRequest.Setup(req => req.Headers.ContainsKey(It.IsAny<string>())).Returns(true);
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns("C5574DEF8B2CC967501C3547FA1E60B9457BF03E");

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
        public async void ListWebhookSubscriptionsAsyncShouldSucceed()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.ListWebhooksAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(webhookList));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var actualList = await webexAdapter.ListWebhookSubscriptionsAsync(new CancellationToken());

            Assert.Equal(webhookList.Items[0].Id, actualList.Items[0].Id);
            Assert.Equal(webhookList.Items[1].Id, actualList.Items[1].Id);
        }

        [Fact]
        public async void ResetWebhookSubscriptionsAsyncShouldSucceed()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));
            var deletedItems = 0;

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.DeleteWebhookAsync(It.IsAny<Webhook>(), It.IsAny<CancellationToken>())).Callback(() =>
            {
                deletedItems++;
            });

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            await webexAdapter.ResetWebhookSubscriptionsAsync(webhookList, new CancellationToken());

            Assert.Equal(webhookList.ItemCount, deletedItems);
        }

        [Fact]
        public async void RegisterWebhookSubscriptionsAsyncUpdateWebhookShouldSucceed()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.ListWebhooksAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(webhookList));
            webexApi.Setup(x => x.UpdateWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(webhookList.Items[1]));
            webexApi.Setup(x => x.UpdateAdaptiveCardsWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(webhookList.Items[0]));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var webhook = await webexAdapter.RegisterWebhookSubscriptionsAsync();

            Assert.Equal(webhookList.Items[0].Id, webhook[1].Id);
            Assert.Equal(webhookList.Items[1].Id, webhook[0].Id);
        }

        [Fact]
        public async void RegisterWebhookSubscriptionAsyncCreateWebhookShouldSucceed()
        {
            var webhookName = "New_Webhook";

            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test", webhookName);

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));
            var webhook = JsonConvert.DeserializeObject<Webhook>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Webhook.json"));
            var webhookCards = JsonConvert.DeserializeObject<Webhook>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookAttachmentActions.json"));

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.ListWebhooksAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(webhookList));
            webexApi.Setup(x => x.CreateWebhookAsync(It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<EventResource>(), It.IsAny<EventType>(), It.IsAny<IEnumerable<EventFilter>>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(webhook));
            webexApi.Setup(x => x.CreateAdaptiveCardsWebhookAsync(It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<EventType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(webhook));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var actualWebhook = await webexAdapter.RegisterWebhookSubscriptionsAsync();

            Assert.Equal(webhook.Id, actualWebhook[0].Id);
            Assert.Equal(webhook.Name, actualWebhook[0].Name);
            Assert.Equal(webhookCards.Id, actualWebhook[1].Id);
            Assert.Equal(webhookCards.Name, actualWebhook[1].Name);
        }

        [Fact]
        public async void UpdateActivityAsyncShouldThrowNotSupportedException()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);

            var activity = new Activity();

            var turnContext = new TurnContext(webexAdapter, activity);

            await Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await webexAdapter.UpdateActivityAsync(turnContext, activity, default);
            });
        }

        [Fact]
        public async void SendActivitiesAsyncShouldFailWithActivityTypeNotMessage()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
            };

            Activity[] activities = { activity };

            var turnContext = new TurnContext(webexAdapter, activity);

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await webexAdapter.SendActivitiesAsync(turnContext, activities, default);
            });
        }

        [Fact]
        public async void SendActivitiesAsyncNotNulltoPersonEmailShouldSucceed()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            // Setup mocked Webex API client
            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.Setup(x => x.CreateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<Uri>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResponseId));

            // Create a new Webex Adapter with the mocked classes and get the responses
            var webexAdapter = new WebexAdapter(options, webexApi.Object);

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
        public async void SendActivitiesAsyncShouldFailWithNulltoPersonEmail()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            // Setup mocked Webex API client
            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.Setup(x => x.CreateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<Uri>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResponseId));

            // Create a new Webex Adapter with the mocked classes and get the responses
            var webexAdapter = new WebexAdapter(options, webexApi.Object);
            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = "message";
            activity.Object.Text = "Hello, Bot!";

            var turnContext = new TurnContext(webexAdapter, activity.Object);

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await webexAdapter.SendActivitiesAsync(turnContext, new Activity[] { activity.Object }, default);
            });
        }

        [Fact]
        public async void SendActivitiesAsyncWithAttachmentShouldSucceed()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            // Setup mocked Webex API client
            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.Setup(x => x.CreateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<Uri>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResponseId));

            // Create a new Webex Adapter with the mocked classes and get the responses
            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = "message";
            activity.Object.Recipient = new ChannelAccount(id: "MockId");
            activity.Object.Text = "Hello, Bot!";
            activity.Object.Attachments = new List<Attachment>
            {
                new Attachment("image/png", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU"),
            };

            var turnContext = new TurnContext(webexAdapter, activity.Object);

            var resourceResponse = await webexAdapter.SendActivitiesAsync(turnContext, new Activity[] { activity.Object }, default).ConfigureAwait(false);

            // Assert the result
            Assert.True(resourceResponse[0].Id == expectedResponseId);
        }

        [Fact]
        public async void SendActivitiesAsyncWithAttachmentActionsShouldSucceed()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.Setup(x => x.CreateMessageWithAttachmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<Attachment>>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResponseId));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = "message";
            activity.Object.Recipient = new ChannelAccount(id: "MockId");
            activity.Object.Text = "Hello, Bot!";
            activity.Object.Attachments = new List<Attachment>
            {
                new Attachment("application/vnd.microsoft.card.adaptive"),
            };

            var turnContext = new TurnContext(webexAdapter, activity.Object);

            var resourceResponse = await webexAdapter.SendActivitiesAsync(turnContext, new Activity[] { activity.Object }, default).ConfigureAwait(false);

            Assert.True(resourceResponse[0].Id == expectedResponseId);
        }

        [Fact]
        public async void DeleteActivityAsyncWithActivityIdShouldSucceed()
        {
            var options = new WebexAdapterOptions("Test", _testPublicAddress, "Test");

            var deletedMessages = 0;

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.Setup(x => x.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback(() => { deletedMessages++; });

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

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
