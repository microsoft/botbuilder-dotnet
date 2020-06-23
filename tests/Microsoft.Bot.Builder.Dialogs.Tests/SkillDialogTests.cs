// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class SkillDialogTests
    {
        [TestMethod]
        public void ConstructorValidationTests()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new SkillDialog(null));
        }

        [TestMethod]
        public async Task BeginDialogOptionsValidation()
        {
            var dialogOptions = new SkillDialogOptions();
            var sut = new SkillDialog(dialogOptions);
            var client = new DialogTestClient(Channels.Test, sut);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await client.SendActivityAsync<IMessageActivity>("irrelevant"), "null options should fail");

            client = new DialogTestClient(Channels.Test, sut, new Dictionary<string, string>());
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await client.SendActivityAsync<IMessageActivity>("irrelevant"), "options should be of type DialogArgs");

            client = new DialogTestClient(Channels.Test, sut, new BeginSkillDialogOptions());
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await client.SendActivityAsync<IMessageActivity>("irrelevant"), "Activity in DialogArgs should be set");
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(DeliveryModes.ExpectReplies)]
        public async Task BeginDialogCallsSkill(string deliveryMode)
        {
            Activity activitySent = null;
            string fromBotIdSent = null;
            string toBotIdSent = null;
            Uri toUriSent = null;

            // Callback to capture the parameters sent to the skill
            void CaptureAction(string fromBotId, string toBotId, Uri toUri, Uri serviceUrl, string conversationId, Activity activity, CancellationToken cancellationToken)
            {
                // Capture values sent to the skill so we can assert the right parameters were used.
                fromBotIdSent = fromBotId;
                toBotIdSent = toBotId;
                toUriSent = toUri;
                activitySent = activity;
            }

            // Create a mock skill client to intercept calls and capture what is sent.
            var mockSkillClient = CreateMockSkillClient(CaptureAction, 200);

            // Use Memory for conversation state
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogOptions = CreateSkillDialogOptions(conversationState, mockSkillClient);

            // Create the SkillDialogInstance and the activity to send.
            var sut = new SkillDialog(dialogOptions);
            var activityToSend = (Activity)Activity.CreateMessageActivity();
            activityToSend.DeliveryMode = deliveryMode;
            activityToSend.Text = Guid.NewGuid().ToString();
            var client = new DialogTestClient(Channels.Test, sut, new BeginSkillDialogOptions { Activity = activityToSend }, conversationState: conversationState);

            // Send something to the dialog to start it
            await client.SendActivityAsync<Activity>("irrelevant");

            // Assert results and data sent to the SkillClient for fist turn
            Assert.AreEqual(dialogOptions.BotId, fromBotIdSent);
            Assert.AreEqual(dialogOptions.Skill.AppId, toBotIdSent);
            Assert.AreEqual(dialogOptions.Skill.SkillEndpoint.ToString(), toUriSent.ToString());
            Assert.AreEqual(activityToSend.Text, activitySent.Text);
            Assert.AreEqual(DialogTurnStatus.Waiting, client.DialogTurnResult.Status);

            // Send a second message to continue the dialog
            await client.SendActivityAsync<Activity>("Second message");

            // Assert results for second turn
            Assert.AreEqual("Second message", activitySent.Text);
            Assert.AreEqual(DialogTurnStatus.Waiting, client.DialogTurnResult.Status);

            // Send EndOfConversation to the dialog
            await client.SendActivityAsync<IEndOfConversationActivity>((Activity)Activity.CreateEndOfConversationActivity());

            // Assert we are done.
            Assert.AreEqual(DialogTurnStatus.Complete, client.DialogTurnResult.Status);
        }

        [TestMethod]
        public async Task ShouldHandleInvokeActivities()
        {
            Activity activitySent = null;
            string fromBotIdSent = null;
            string toBotIdSent = null;
            Uri toUriSent = null;

            // Callback to capture the parameters sent to the skill
            void CaptureAction(string fromBotId, string toBotId, Uri toUri, Uri serviceUrl, string conversationId, Activity activity, CancellationToken cancellationToken)
            {
                // Capture values sent to the skill so we can assert the right parameters were used.
                fromBotIdSent = fromBotId;
                toBotIdSent = toBotId;
                toUriSent = toUri;
                activitySent = activity;
            }

            // Create a mock skill client to intercept calls and capture what is sent.
            var mockSkillClient = CreateMockSkillClient(CaptureAction);

            // Use Memory for conversation state
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogOptions = CreateSkillDialogOptions(conversationState, mockSkillClient);

            // Create the SkillDialogInstance and the activity to send.
            var activityToSend = (Activity)Activity.CreateInvokeActivity();
            activityToSend.Name = Guid.NewGuid().ToString();
            var sut = new SkillDialog(dialogOptions);
            var client = new DialogTestClient(Channels.Test, sut, new BeginSkillDialogOptions { Activity = activityToSend }, conversationState: conversationState);

            // Send something to the dialog to start it
            await client.SendActivityAsync<Activity>("irrelevant");

            // Assert results and data sent to the SkillClient for fist turn
            Assert.AreEqual(dialogOptions.BotId, fromBotIdSent);
            Assert.AreEqual(dialogOptions.Skill.AppId, toBotIdSent);
            Assert.AreEqual(dialogOptions.Skill.SkillEndpoint.ToString(), toUriSent.ToString());
            Assert.AreEqual(activityToSend.Name, activitySent.Name);
            Assert.AreEqual(DeliveryModes.ExpectReplies, activitySent.DeliveryMode);
            Assert.AreEqual(DialogTurnStatus.Waiting, client.DialogTurnResult.Status);

            // Send a second message to continue the dialog
            await client.SendActivityAsync<Activity>("Second message");

            // Assert results for second turn
            Assert.AreEqual("Second message", activitySent.Text);
            Assert.AreEqual(DialogTurnStatus.Waiting, client.DialogTurnResult.Status);

            // Send EndOfConversation to the dialog
            await client.SendActivityAsync<IEndOfConversationActivity>((Activity)Activity.CreateEndOfConversationActivity());

            // Assert we are done.
            Assert.AreEqual(DialogTurnStatus.Complete, client.DialogTurnResult.Status);
        }

        [TestMethod]
        public async Task CancelDialogSendsEoC()
        {
            Activity activitySent = null;

            // Callback to capture the parameters sent to the skill
            void CaptureAction(string fromBotId, string toBotId, Uri toUri, Uri serviceUrl, string conversationId, Activity activity, CancellationToken cancellationToken)
            {
                // Capture values sent to the skill so we can assert the right parameters were used.
                activitySent = activity;
            }

            // Create a mock skill client to intercept calls and capture what is sent.
            var mockSkillClient = CreateMockSkillClient(CaptureAction);

            // Use Memory for conversation state
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogOptions = CreateSkillDialogOptions(conversationState, mockSkillClient);

            // Create the SkillDialogInstance and the activity to send.
            var sut = new SkillDialog(dialogOptions);
            var activityToSend = (Activity)Activity.CreateMessageActivity();
            activityToSend.Text = Guid.NewGuid().ToString();
            var client = new DialogTestClient(Channels.Test, sut, new BeginSkillDialogOptions { Activity = activityToSend }, conversationState: conversationState);

            // Send something to the dialog to start it
            await client.SendActivityAsync<IMessageActivity>("irrelevant");

            // Cancel the dialog so it sends an EoC to the skill
            await client.DialogContext.CancelAllDialogsAsync(CancellationToken.None);

            Assert.AreEqual(ActivityTypes.EndOfConversation, activitySent.Type);
        }

        [TestMethod]
        public async Task ShouldThrowHttpExceptionOnPostFailure()
        {
            // Create a mock skill client to intercept calls and capture what is sent.
            var mockSkillClient = CreateMockSkillClient(null, 500);

            // Use Memory for conversation state
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogOptions = CreateSkillDialogOptions(conversationState, mockSkillClient);

            // Create the SkillDialogInstance and the activity to send.
            var sut = new SkillDialog(dialogOptions);
            var activityToSend = (Activity)Activity.CreateMessageActivity();
            activityToSend.Text = Guid.NewGuid().ToString();
            var client = new DialogTestClient(Channels.Test, sut, new BeginSkillDialogOptions { Activity = activityToSend }, conversationState: conversationState);

            // Send something to the dialog 
            await Assert.ThrowsExceptionAsync<HttpRequestException>(async () => await client.SendActivityAsync<IMessageActivity>("irrelevant"));
        }

        [TestMethod]
        public async Task ShouldInterceptOAuthCardsForSso()
        {
            var connectionName = "connectionName";
            var firstResponse = new ExpectedReplies(new List<Activity>() { CreateOAuthCardAttachmentActivity("https://test") });
            var mockSkillClient = new Mock<BotFrameworkClient>();
            mockSkillClient
                .SetupSequence(x => x.PostActivityAsync<ExpectedReplies>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InvokeResponse<ExpectedReplies> { Status = 200, Body = firstResponse }))
                .Returns(Task.FromResult(new InvokeResponse<ExpectedReplies> { Status = 200 }));

            var conversationState = new ConversationState(new MemoryStorage());
            var testAdapter = new TestAdapter(Channels.Test)
                .Use(new AutoSaveStateMiddleware(conversationState));

            var dialogOptions = CreateSkillDialogOptions(conversationState, mockSkillClient, connectionName);
            var sut = new SkillDialog(dialogOptions);
            var activityToSend = CreateSendActivity();
            var client = new DialogTestClient(testAdapter, sut, new BeginSkillDialogOptions { Activity = activityToSend }, conversationState: conversationState);
            testAdapter.AddExchangeableToken(connectionName, Channels.Test, "user1", "https://test", "https://test1");
            var finalActivity = await client.SendActivityAsync<IMessageActivity>("irrelevant");
            Assert.IsNull(finalActivity);
        }

        [TestMethod]
        public async Task ShouldNotInterceptOAuthCardsForEmptyConnectionName()
        {
            var connectionName = "connectionName";
            var firstResponse = new ExpectedReplies(new List<Activity> { CreateOAuthCardAttachmentActivity("https://test") });
            var mockSkillClient = new Mock<BotFrameworkClient>();
            mockSkillClient
                .Setup(x => x.PostActivityAsync<ExpectedReplies>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InvokeResponse<ExpectedReplies>
                {
                    Status = 200,
                    Body = firstResponse
                }));

            var conversationState = new ConversationState(new MemoryStorage());
            var dialogOptions = CreateSkillDialogOptions(conversationState, mockSkillClient);

            var sut = new SkillDialog(dialogOptions);
            var activityToSend = CreateSendActivity();
            var testAdapter = new TestAdapter(Channels.Test)
                .Use(new AutoSaveStateMiddleware(conversationState));
            var client = new DialogTestClient(testAdapter, sut, new BeginSkillDialogOptions { Activity = activityToSend }, conversationState: conversationState);
            testAdapter.AddExchangeableToken(connectionName, Channels.Test, "user1", "https://test", "https://test1");
            var finalActivity = await client.SendActivityAsync<IMessageActivity>("irrelevant");
            Assert.IsNotNull(finalActivity);
            Assert.IsTrue(finalActivity.Attachments.Count == 1);
        }

        [TestMethod]
        public async Task ShouldNotInterceptOAuthCardsForEmptyToken()
        {
            var firstResponse = new ExpectedReplies(new List<Activity>() { CreateOAuthCardAttachmentActivity("https://test") });
            var mockSkillClient = new Mock<BotFrameworkClient>();
            mockSkillClient
                .Setup(x => x.PostActivityAsync<ExpectedReplies>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InvokeResponse<ExpectedReplies> { Status = 200, Body = firstResponse }));

            var conversationState = new ConversationState(new MemoryStorage());
            var dialogOptions = CreateSkillDialogOptions(conversationState, mockSkillClient);

            var sut = new SkillDialog(dialogOptions);
            var activityToSend = CreateSendActivity();
            var testAdapter = new TestAdapter(Channels.Test)
            .Use(new AutoSaveStateMiddleware(conversationState));
            var client = new DialogTestClient(testAdapter, sut, new BeginSkillDialogOptions { Activity = activityToSend }, conversationState: conversationState);

            // Don't add exchangeable token to test adapter
            var finalActivity = await client.SendActivityAsync<IMessageActivity>("irrelevant");
            Assert.IsNotNull(finalActivity);
            Assert.IsTrue(finalActivity.Attachments.Count == 1);
        }

        [TestMethod]
        public async Task ShouldNotInterceptOAuthCardsForTokenException()
        {
            var connectionName = "connectionName";
            var firstResponse = new ExpectedReplies(new List<Activity> { CreateOAuthCardAttachmentActivity("https://test") });
            var mockSkillClient = new Mock<BotFrameworkClient>();
            mockSkillClient
                .Setup(x => x.PostActivityAsync<ExpectedReplies>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InvokeResponse<ExpectedReplies>
                {
                    Status = 200,
                    Body = firstResponse
                }));

            var conversationState = new ConversationState(new MemoryStorage());
            var dialogOptions = CreateSkillDialogOptions(conversationState, mockSkillClient, connectionName);

            var sut = new SkillDialog(dialogOptions);
            var activityToSend = CreateSendActivity();
            var testAdapter = new TestAdapter(Channels.Test)
                .Use(new AutoSaveStateMiddleware(conversationState));
            var initialDialogOptions = new BeginSkillDialogOptions { Activity = activityToSend };
            var client = new DialogTestClient(testAdapter, sut, initialDialogOptions, conversationState: conversationState);
            testAdapter.ThrowOnExchangeRequest(connectionName, Channels.Test, "user1", "https://test");
            var finalActivity = await client.SendActivityAsync<IMessageActivity>("irrelevant");
            Assert.IsNotNull(finalActivity);
            Assert.IsTrue(finalActivity.Attachments.Count == 1);
        }

        [TestMethod]
        public async Task ShouldNotInterceptOAuthCardsForBadRequest()
        {
            var connectionName = "connectionName";
            var firstResponse = new ExpectedReplies(new List<Activity>() { CreateOAuthCardAttachmentActivity("https://test") });
            var mockSkillClient = new Mock<BotFrameworkClient>();
            mockSkillClient
                .SetupSequence(x => x.PostActivityAsync<ExpectedReplies>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InvokeResponse<ExpectedReplies> { Status = 200, Body = firstResponse }))
                .Returns(Task.FromResult(new InvokeResponse<ExpectedReplies> { Status = 409 }));

            var conversationState = new ConversationState(new MemoryStorage());
            var dialogOptions = SkillDialogTests.CreateSkillDialogOptions(conversationState, mockSkillClient, connectionName);

            var sut = new SkillDialog(dialogOptions);
            var activityToSend = CreateSendActivity();
            var testAdapter = new TestAdapter(Channels.Test)
                .Use(new AutoSaveStateMiddleware(conversationState));
            var client = new DialogTestClient(testAdapter, sut, new BeginSkillDialogOptions { Activity = activityToSend }, conversationState: conversationState);
            testAdapter.AddExchangeableToken(connectionName, Channels.Test, "user1", "https://test", "https://test1");
            var finalActivity = await client.SendActivityAsync<IMessageActivity>("irrelevant");
            Assert.IsNotNull(finalActivity);
            Assert.IsTrue(finalActivity.Attachments.Count == 1);
        }

        private static Activity CreateOAuthCardAttachmentActivity(string uri)
        {
            var oauthCard = new OAuthCard()
            {
                TokenExchangeResource = new TokenExchangeResource()
                {
                    Uri = uri
                }
            };
            var attachment = new Attachment()
            {
                ContentType = OAuthCard.ContentType,
                Content = JObject.FromObject(oauthCard)
            };

            var attachmentActivity = MessageFactory.Attachment(attachment);
            attachmentActivity.Conversation = new ConversationAccount() { Id = Guid.NewGuid().ToString() };
            attachmentActivity.From = new ChannelAccount("blah", "name");

            return attachmentActivity as Activity;
        }

        /// <summary>
        /// Helper to create a <see cref="SkillDialogOptions"/> for the skillDialog.
        /// </summary>
        /// <param name="conversationState"> The conversation state object.</param>
        /// <param name="mockSkillClient"> The skill client mock.</param>
        /// <returns> A Skill Dialog Options object.</returns>
        private static SkillDialogOptions CreateSkillDialogOptions(ConversationState conversationState, Mock<BotFrameworkClient> mockSkillClient, string connectionName = null)
        {
            var dialogOptions = new SkillDialogOptions
            {
                BotId = Guid.NewGuid().ToString(),
                SkillHostEndpoint = new Uri("http://test.contoso.com/skill/messages"),
                ConversationIdFactory = new SimpleConversationIdFactory(),
                ConversationState = conversationState,
                SkillClient = mockSkillClient.Object,
                Skill = new BotFrameworkSkill
                {
                    AppId = Guid.NewGuid().ToString(),
                    SkillEndpoint = new Uri("http://testskill.contoso.com/api/messages")
                },
                ConnectionName = connectionName
            };
            return dialogOptions;
        }

        private static Mock<BotFrameworkClient> CreateMockSkillClient(Action<string, string, Uri, Uri, string, Activity, CancellationToken> captureAction, int returnStatus = 200)
        {
            var mockSkillClient = new Mock<BotFrameworkClient>();
            var activityList = new ExpectedReplies(new List<Activity> { MessageFactory.Text("dummy activity") });

            if (captureAction != null)
            {
                mockSkillClient
                    .Setup(x => x.PostActivityAsync<ExpectedReplies>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(new InvokeResponse<ExpectedReplies> { Status = returnStatus, Body = activityList }))
                    .Callback(captureAction);
            }
            else
            {
                mockSkillClient
                    .Setup(x => x.PostActivityAsync<ExpectedReplies>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(new InvokeResponse<ExpectedReplies> { Status = returnStatus, Body = activityList }));
            }

            return mockSkillClient;
        }

        private Activity CreateSendActivity()
        {
            var activityToSend = (Activity)Activity.CreateMessageActivity();
            activityToSend.DeliveryMode = DeliveryModes.ExpectReplies;
            activityToSend.Text = Guid.NewGuid().ToString();
            return activityToSend;
        }

        // Simple conversation ID factory for testing.
        private class SimpleConversationIdFactory : SkillConversationIdFactoryBase
        {
            private readonly ConcurrentDictionary<string, SkillConversationReference> _conversationRefs = new ConcurrentDictionary<string, SkillConversationReference>();

            public override Task<string> CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions options, CancellationToken cancellationToken)
            {
                var key = (options.Activity.Conversation.Id + options.Activity.ServiceUrl).GetHashCode().ToString(CultureInfo.InvariantCulture);
                _conversationRefs.GetOrAdd(key, new SkillConversationReference
                {
                    ConversationReference = options.Activity.GetConversationReference(),
                    OAuthScope = options.FromBotOAuthScope
                });
                return Task.FromResult(key);
            }

            public override Task<SkillConversationReference> GetSkillConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                return Task.FromResult(_conversationRefs[skillConversationId]);
            }

            public override Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
