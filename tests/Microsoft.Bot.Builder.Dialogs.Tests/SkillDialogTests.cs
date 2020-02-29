// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

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

            client = new DialogTestClient(Channels.Test, sut, new SkillDialogArgs());
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await client.SendActivityAsync<IMessageActivity>("irrelevant"), "Activity in DialogArgs should be set");

            client = new DialogTestClient(Channels.Test, sut, new SkillDialogArgs { Activity = (Activity)Activity.CreateConversationUpdateActivity() });
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await client.SendActivityAsync<IMessageActivity>("irrelevant"), "Only Message and Event activities are supported");
        }

        [TestMethod]
        public async Task BeginDialogCallsSkill()
        {
            Activity sentActivity = null;

            // Create a mock skill client to intercept calls and capture what is sent.
            var mockSkillClient = new Mock<BotFrameworkClient>();
            mockSkillClient
                .Setup(x => x.PostActivityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InvokeResponse { Status = 200 }))
                .Callback<string, string, Uri, Uri, string, Activity, CancellationToken>((fromBotId, toBotId, toUri, serviceUrl, conversationId, activity, cancellationToken) =>
                {
                    // Capture values sent to the skill so we can assert the right parameters were used.
                    sentActivity = activity;
                });

            // Use Memory for conversation state
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogOptions = CreateSkillDialogOptions(conversationState, mockSkillClient);

            // Create the SkillDialogInstance and the activity to send.
            var sut = new SkillDialog(dialogOptions);
            var activityToSend = (Activity)Activity.CreateMessageActivity();
            activityToSend.Text = Guid.NewGuid().ToString();
            var client = new DialogTestClient(Channels.Test, sut, new SkillDialogArgs { Activity = activityToSend }, conversationState: conversationState);

            // Send something to the dialog to start it
            await client.SendActivityAsync<IMessageActivity>("irrelevant");

            // Assert results
            Assert.AreEqual(activityToSend.Text, sentActivity.Text);
            Assert.AreEqual(DialogTurnStatus.Waiting, client.DialogTurnResult.Status);

            // Send a second message to continue the dialog
            await client.SendActivityAsync<IMessageActivity>("Second message");

            // Assert results
            Assert.AreEqual("Second message", sentActivity.Text);
            Assert.AreEqual(DialogTurnStatus.Waiting, client.DialogTurnResult.Status);

            // Send EndOfConversation to the dialog
            await client.SendActivityAsync<IEndOfConversationActivity>((Activity)Activity.CreateEndOfConversationActivity());

            // Assert we are done.
            Assert.AreEqual(DialogTurnStatus.Complete, client.DialogTurnResult.Status);
        }

        [TestMethod]
        public async Task CancelDialogSendsEoC()
        {
            Activity sentActivity = null;

            // Create a mock skill client to intercept calls and capture what is sent.
            var mockSkillClient = new Mock<BotFrameworkClient>();
            mockSkillClient
                .Setup(x => x.PostActivityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new InvokeResponse { Status = 200 }))
                .Callback<string, string, Uri, Uri, string, Activity, CancellationToken>((fromBotId, toBotId, toUri, serviceUrl, conversationId, activity, cancellationToken) =>
                {
                    // Capture values sent to the skill so we can assert the right parameters were used.
                    sentActivity = activity;
                });

            // Use Memory for conversation state
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogOptions = CreateSkillDialogOptions(conversationState, mockSkillClient);

            // Create the SkillDialogInstance and the activity to send.
            var sut = new SkillDialog(dialogOptions);
            var activityToSend = (Activity)Activity.CreateMessageActivity();
            activityToSend.Text = Guid.NewGuid().ToString();
            var client = new DialogTestClient(Channels.Test, sut, new SkillDialogArgs { Activity = activityToSend }, conversationState: conversationState);

            // Send something to the dialog to start it
            await client.SendActivityAsync<IMessageActivity>("irrelevant");

            // Cancel the dialog so it sends an EoC to the skill
            await client.DialogContext.CancelAllDialogsAsync(CancellationToken.None);

            Assert.AreEqual(ActivityTypes.EndOfConversation, sentActivity.Type);
        }

        /// <summary>
        /// Helper to create a <see cref="SkillDialogOptions"/> for the skillDialog.
        /// </summary>
        private static SkillDialogOptions CreateSkillDialogOptions(ConversationState conversationState, Mock<BotFrameworkClient> mockSkillClient)
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
                }
            };
            return dialogOptions;
        }

        // Simple conversation ID factory for testing.
        private class SimpleConversationIdFactory : SkillConversationIdFactoryBase
        {
            private readonly ConcurrentDictionary<string, string> _conversationRefs = new ConcurrentDictionary<string, string>();

            public override Task<string> CreateSkillConversationIdAsync(ConversationReference conversationReference, CancellationToken cancellationToken)
            {
                var crJson = JsonConvert.SerializeObject(conversationReference);
                var key = (conversationReference.Conversation.Id + conversationReference.ServiceUrl).GetHashCode().ToString(CultureInfo.InvariantCulture);
                _conversationRefs.GetOrAdd(key, crJson);
                return Task.FromResult(key);
            }

            public override Task<ConversationReference> GetConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                var conversationReference = JsonConvert.DeserializeObject<ConversationReference>(_conversationRefs[skillConversationId]);
                return Task.FromResult(conversationReference);
            }

            public override Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
