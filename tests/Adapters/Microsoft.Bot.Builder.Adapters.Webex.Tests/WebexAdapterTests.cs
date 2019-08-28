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
        public async void GetIdentityAsync_Should_Succeed()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var person = JsonConvert.DeserializeObject<Person>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Person.json"));

            var webexApi = new Mock<IWebexClient>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMeAsync()).Returns(Task.FromResult(person));

            var webexAdapter = new WebexAdapter(options.Object, webexApi.Object);
            await webexAdapter.GetIdentityAsync();

            Assert.Equal(person.Id, WebexHelper.Identity.Id);
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
        public async void ProcessAsync_With_EvenType_Created_Should_Succeed()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Message.json"));
            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Payload.json");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));
            var call = false;

            WebexHelper.Identity = JsonConvert.DeserializeObject<Person>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Person.json"));

            var webexApi = new Mock<IWebexClient>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.GetMessageAsync(It.IsAny<string>(), default)).Returns(Task.FromResult(message));

            var webexAdapter = new WebexAdapter(options.Object, webexApi.Object);

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
        public async void ProcessAsync_With_EvenType_Updated_Should_Succeed()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object, new Mock<IWebexClient>().Object);

            WebexHelper.Identity = JsonConvert.DeserializeObject<Person>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Person.json"));
            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Payload2.json");
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
        public async void ListWebhookSubscriptionsAsync_Should_Succeed()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));

            var webexApi = new Mock<IWebexClient>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.ListWebhooksAsync()).Returns(Task.FromResult(webhookList));

            var webexAdapter = new WebexAdapter(options.Object, webexApi.Object);

            var actualList = await webexAdapter.ListWebhookSubscriptionsAsync();

            Assert.Equal(webhookList.Items[0].Id, actualList.Items[0].Id);
            Assert.Equal(webhookList.Items[1].Id, actualList.Items[1].Id);
        }

        [Fact]
        public async void ResetWebhookSubscriptionsAsync_Should_Succeed()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));
            var deletedItems = 0;

            var webexApi = new Mock<IWebexClient>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.DeleteWebhookAsync(It.IsAny<Webhook>())).Callback(() =>
            {
                deletedItems++;
            });

            var webexAdapter = new WebexAdapter(options.Object, webexApi.Object);

            await webexAdapter.ResetWebhookSubscriptionsAsync(webhookList);

            Assert.Equal(webhookList.ItemCount, deletedItems);
        }

        [Fact]
        public async void RegisterWebhookSubscriptionAsync_UpdateWebhook_Should_Succeed()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));

            var webexApi = new Mock<IWebexClient>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.UpdateWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<string>())).Returns(Task.FromResult(webhookList.Items[1]));

            var webexAdapter = new WebexAdapter(options.Object, webexApi.Object);

            var webhook = await webexAdapter.RegisterWebhookSubscriptionAsync("api/messages", webhookList);

            Assert.Equal(webhookList.Items[1].Id, webhook.Id);
        }

        [Fact]
        public async void RegisterWebhookSubscriptionAsync_CreateWebhook_Should_Succeed()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";
            options.Object.WebhookName = "New_Webhook";

            var webhookList = JsonConvert.DeserializeObject<WebhookList>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\WebhookList.json"));
            var webhook = JsonConvert.DeserializeObject<Webhook>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\Webhook.json"));

            var webexApi = new Mock<IWebexClient>();
            webexApi.SetupAllProperties();
            webexApi.Setup(x => x.CreateWebhookAsync(It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<EventResource>(), It.IsAny<EventType>(), It.IsAny<IEnumerable<EventFilter>>(), It.IsAny<string>())).Returns(Task.FromResult(webhook));

            var webexAdapter = new WebexAdapter(options.Object, webexApi.Object);

            var actualWebhook = await webexAdapter.RegisterWebhookSubscriptionAsync("api/messages", webhookList);

            Assert.Equal(webhook.Id, actualWebhook.Id);
            Assert.Equal(webhook.Name, actualWebhook.Name);
        }

        [Fact]
        public async void UpdateActivityAsync_Should_Throw_NotSupportedException()
        {
            var options = new Mock<IWebexAdapterOptions>();
            options.SetupAllProperties();
            options.Object.AccessToken = "Test";
            options.Object.PublicAddress = "http://contoso.com/api/messages";

            var webexAdapter = new WebexAdapter(options.Object, new Mock<IWebexClient>().Object);

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

            var deletedMessages = 0;

            var webexApi = new Mock<IWebexClient>();
            webexApi.Setup(x => x.DeleteMessageAsync(It.IsAny<string>())).Callback(() => { deletedMessages++; });

            var webexAdapter = new WebexAdapter(options.Object, webexApi.Object);

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
