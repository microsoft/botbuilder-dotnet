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
        [Fact]
        public void Constructor_Should_Fail_With_Null_Config()
        {
            Assert.Throws<ArgumentNullException>(() => { new WebexAdapter(null, new Mock<WebexClientWrapper>().Object); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_AccessToken()
        {
            var options = new WebexAdapterOptions(null, "Test", "Test");

            Assert.Throws<Exception>(() => { new WebexAdapter(options, new Mock<WebexClientWrapper>().Object); });
        }

        [Fact]
        public void Constructor_Should_Fail_With_Null_PublicAddress()
        {
            var options = new WebexAdapterOptions("Test", null, "Test");

            Assert.Throws<Exception>(() => { new WebexAdapter(options, new Mock<WebexClientWrapper>().Object); });
        }

        [Fact]
        public void Constructor_WithArguments_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            Assert.NotNull(new WebexAdapter(options, new Mock<WebexClientWrapper>().Object));
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Fail_With_Null_ConversationReference()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);

            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await webexAdapter.ContinueConversationAsync(null, BotsLogic, default); });
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Fail_With_Null_Logic()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);
            var conversationReference = new ConversationReference();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await webexAdapter.ContinueConversationAsync(conversationReference, null, default); });
        }

        [Fact]
        public async void ContinueConversationAsync_Should_Succeed()
        {
            var callbackInvoked = false;
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

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
        public async void GetIdentityAsync_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var person = JsonConvert.DeserializeObject<Person>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Person.json"));

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMeAsync()).Returns(Task.FromResult(person));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);
            await webexAdapter.GetIdentityAsync();

            Assert.Equal(person.Id, WebexHelper.Identity.Id);
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_HttpRequest()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await webexAdapter.ProcessAsync(null, httpResponse.Object, bot.Object, default);
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_HttpResponse()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);
            var httpRequest = new Mock<HttpRequest>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await webexAdapter.ProcessAsync(httpRequest.Object, null, default, default);
            });
        }

        [Fact]
        public async void ProcessAsync_Should_Fail_With_Null_Bot()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await webexAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, null, default);
            });
        }

        [Fact]
        public async void ProcessAsync_With_EvenType_Created_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Message.json"));
            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Payload.json");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));
            var call = false;

            WebexHelper.Identity = JsonConvert.DeserializeObject<Person>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Person.json"));

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMessageAsync(It.IsAny<string>(), default)).Returns(Task.FromResult(message));

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
        public async void ProcessAsync_With_EvenType_Updated_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);

            WebexHelper.Identity = JsonConvert.DeserializeObject<Person>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Person.json"));
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
        public async void ProcessAsync_Should_Fail_With_NonMatching_Signature()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);

            WebexHelper.Identity = JsonConvert.DeserializeObject<Person>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Person.json"));
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
        public async void ProcessAsync_With_AttachmentActions_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\MessageWithInputs.json"));
            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\PayloadAttachmentActions.json");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));
            var call = false;

            WebexHelper.Identity = JsonConvert.DeserializeObject<Person>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Person.json"));

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetAttachmentActionAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(message));

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

            await webexAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default);

            Assert.True(call);
        }

        [Fact]
        public async void ListWebhookSubscriptionsAsync_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.ListWebhooksAsync()).Returns(Task.FromResult(webhookList));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var actualList = await webexAdapter.ListWebhookSubscriptionsAsync();

            Assert.Equal(webhookList.Items[0].Id, actualList.Items[0].Id);
            Assert.Equal(webhookList.Items[1].Id, actualList.Items[1].Id);
        }

        [Fact]
        public async void ResetWebhookSubscriptionsAsync_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));
            var deletedItems = 0;

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.DeleteWebhookAsync(It.IsAny<Webhook>())).Callback(() =>
            {
                deletedItems++;
            });

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            await webexAdapter.ResetWebhookSubscriptionsAsync(webhookList);

            Assert.Equal(webhookList.ItemCount, deletedItems);
        }

        [Fact]
        public async void RegisterWebhookSubscriptionAsync_UpdateWebhook_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.UpdateWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<string>())).Returns(Task.FromResult(webhookList.Items[1]));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var webhook = await webexAdapter.RegisterWebhookSubscriptionAsync("api/messages", webhookList);

            Assert.Equal(webhookList.Items[1].Id, webhook.Id);
        }

        [Fact]
        public async void RegisterWebhookSubscriptionAsync_CreateWebhook_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";
            var webhookName = "New_Webhook";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test", webhookName);

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));
            var webhook = JsonConvert.DeserializeObject<Webhook>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Webhook.json"));

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.CreateWebhookAsync(It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<EventResource>(), It.IsAny<EventType>(), It.IsAny<IEnumerable<EventFilter>>(), It.IsAny<string>())).Returns(Task.FromResult(webhook));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var actualWebhook = await webexAdapter.RegisterWebhookSubscriptionAsync("api/messages", webhookList);

            Assert.Equal(webhook.Id, actualWebhook.Id);
            Assert.Equal(webhook.Name, actualWebhook.Name);
        }

        [Fact]
        public async void UpdateActivityAsync_Should_Throw_NotSupportedException()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var webexAdapter = new WebexAdapter(options, new Mock<WebexClientWrapper>().Object);

            var activity = new Activity();

            var turnContext = new TurnContext(webexAdapter, activity);

            await Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await webexAdapter.UpdateActivityAsync(turnContext, activity, default);
            });
        }

        [Fact]
        public async void SendActivitiesAsync_Should_Fail_With_ActivityType_Not_Message()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

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
        public async void SendActivitiesAsync_NotNull_toPersonEmail_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            // Setup mocked Webex API client
            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.Setup(x => x.CreateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<Uri>>())).Returns(Task.FromResult(expectedResponseId));

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
        public async void SendActivitiesAsync_Should_Fail_With_Null_toPersonEmail()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            // Setup mocked Webex API client
            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.Setup(x => x.CreateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<Uri>>())).Returns(Task.FromResult(expectedResponseId));

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
        public async void SendActivitiesAsync_With_Attachment_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            // Setup mocked Webex API client
            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.Setup(x => x.CreateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<Uri>>())).Returns(Task.FromResult(expectedResponseId));

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
        public async void SendActivitiesAsync_With_AttachmentActions_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            const string expectedResponseId = "Mocked Response Id";
            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.Setup(x => x.CreateMessageWithAttachmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<Attachment>>(), It.IsAny<string>())).Returns(Task.FromResult(expectedResponseId));

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
        public async void DeleteActivityAsync_With_ActivityId_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var deletedMessages = 0;

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.Setup(x => x.DeleteMessageAsync(It.IsAny<string>())).Callback(() => { deletedMessages++; });

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

        [Fact]
        public async void RegisterAdaptiveCardsWebhookSubscriptionAsync_CreateWebhook_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";
            var webhookName = "New_Webhook";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test", webhookName);

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));
            var webhook = JsonConvert.DeserializeObject<Webhook>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookAttachmentActions.json"));

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.CreateAdaptiveCardsWebhookAsync(It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<EventType>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(webhook));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var actualWebhook = await webexAdapter.RegisterAdaptiveCardsWebhookSubscriptionAsync("api/messages", webhookList);

            Assert.Equal(webhook.Id, actualWebhook.Id);
            Assert.Equal(webhook.Name, actualWebhook.Name);
        }

        [Fact]
        public async void RegisterAdaptiveCardsWebhookSubscriptionAsync_UpdateWebhook_Should_Succeed()
        {
            var testPublicAddress = "http://contoso.com/api/messages";

            var options = new WebexAdapterOptions("Test", testPublicAddress, "Test");

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));

            var webexApi = new Mock<WebexClientWrapper>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.UpdateAdaptiveCardsWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(webhookList.Items[0]));

            var webexAdapter = new WebexAdapter(options, webexApi.Object);

            var webhook = await webexAdapter.RegisterAdaptiveCardsWebhookSubscriptionAsync("api/messages", webhookList);

            Assert.Equal(webhookList.Items[0].Id, webhook.Id);
        }
    }
}
