// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Middleware")]
    [TestCategory("Functional Spec")]
    public class Middleware_BindOutgoingResponsesTests
    {
        [TestMethod]
        [TestCategory("Functional Spec")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ApplyConversationReference_NullActivity()
        {
            Activity activity = null;
            ConversationReference reference = new ConversationReference();
            BindOutoingResponsesMiddlware.ApplyConversationReference(activity, reference);

            Assert.Fail("Actity was null. This should not run.");
        }

        [TestMethod]
        [TestCategory("Functional Spec")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ApplyConversationReference_NullConversationReference()
        {
            Activity activity = new Activity();
            BindOutoingResponsesMiddlware.ApplyConversationReference(activity, null);

            Assert.Fail("ConversationReference was null. This should not run.");
        }

        [TestMethod]
        [TestCategory("Functional Spec")]
        public async Task FixupActivityType()
        {
            Activity activity = new Activity();
            ConversationReference reference = CreateTestConversationReference();

            // Should apply all relevent properties to the Activity
            BindOutoingResponsesMiddlware.ApplyConversationReference(activity, reference);

            Assert.IsTrue(activity.ChannelId == reference.ChannelId);
            Assert.IsTrue(activity.ServiceUrl == reference.ServiceUrl);
            Assert.IsTrue(activity.Conversation.Id == reference.Conversation.Id);
            Assert.IsTrue(activity.Conversation.Name == reference.Conversation.Name);
            Assert.IsTrue(activity.From.Id == reference.Bot.Id);
            Assert.IsTrue(activity.From.Name == reference.Bot.Name);
            Assert.IsTrue(activity.Recipient.Id == reference.User.Id);
            Assert.IsTrue(activity.Recipient.Name == reference.User.Name);
            Assert.IsTrue(activity.ReplyToId == reference.ActivityId);
        }

        public static ConversationReference CreateTestConversationReference()
        {
            ChannelAccount botAccount = new ChannelAccount("testBotId", "testBotName");
            ChannelAccount userAccount = new ChannelAccount("testUserId", "testUserName");
            ConversationAccount conversationAccount = new ConversationAccount(false, "testConversationAccount", "testConversationName");

            ConversationReference reference = new ConversationReference()
            {
                ChannelId = "testChannelId",
                ServiceUrl = $"https://testServiceUrl",
                Conversation = conversationAccount,
                Bot = botAccount,
                User = userAccount,
                ActivityId = "testActivityId"
            };

            return reference;
        }


        [TestMethod]
        public async Task FixupMessageType()
        {
            IActivity a = new Activity();

            TestAdapter adapter = new TestAdapter();
            Bot b = new Bot(adapter);
            b.OnReceive(async (context) =>
               {
                   Assert.IsTrue(string.IsNullOrEmpty(a.Type));
                   context.Responses.Add(a);
                   Assert.IsTrue(string.IsNullOrEmpty(a.Type)); 
               });

            await adapter
               .Send("test")
               .StartTest();

            // The Message Binder Middleware should have detected an activiy
            // with no message type and set it to be a "Message". 
            Assert.IsTrue(a.Type == ActivityTypes.Message);
        }
    }
}