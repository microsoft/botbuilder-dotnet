// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.Tests
{
    [TestClass]
    public class ActivityTest
    {
        [TestMethod]
        public void GetConversationReference()
        {
            var activity = CreateActivity();

            var conversationReference = activity.GetConversationReference();

            Assert.AreEqual(activity.Id, conversationReference.ActivityId);
            Assert.AreEqual(activity.From.Id, conversationReference.User.Id);
            Assert.AreEqual(activity.Recipient.Id, conversationReference.Bot.Id);
            Assert.AreEqual(activity.Conversation.Id, conversationReference.Conversation.Id);
            Assert.AreEqual(activity.ChannelId, conversationReference.ChannelId);
            Assert.AreEqual(activity.Locale, conversationReference.Locale);
            Assert.AreEqual(activity.ServiceUrl, conversationReference.ServiceUrl);
        }

        [TestMethod]
        public void GetReplyConversationReference()
        {
            var activity = CreateActivity();

            var reply = new ResourceResponse
            {
                Id = "1234",
            };

            var conversationReference = activity.GetReplyConversationReference(reply);

            Assert.AreEqual(reply.Id, conversationReference.ActivityId);
            Assert.AreEqual(activity.From.Id, conversationReference.User.Id);
            Assert.AreEqual(activity.Recipient.Id, conversationReference.Bot.Id);
            Assert.AreEqual(activity.Conversation.Id, conversationReference.Conversation.Id);
            Assert.AreEqual(activity.ChannelId, conversationReference.ChannelId);
            Assert.AreEqual(activity.Locale, conversationReference.Locale);
            Assert.AreEqual(activity.ServiceUrl, conversationReference.ServiceUrl);
        }

        [TestMethod]
        public void RemoveRecipientMention_forTeams()
        {
            var activity = CreateActivity();
            activity.Text = "<at>firstName</at> lastName\n";
            var expectedStrippedName = "lastName";

            var mention = new Mention
            {
                Mentioned = new ChannelAccount()
                {
                    Id = activity.Recipient.Id,
                    Name = "firstName",
                },
                Text = null,
                Type = "mention",
            };
            var lst = new List<Entity>();

            var output = JsonConvert.SerializeObject(mention);
            var entity = JsonConvert.DeserializeObject<Entity>(output);
            lst.Add(entity);
            activity.Entities = lst;

            var strippedActivityText = activity.RemoveRecipientMention();
            Assert.AreEqual(strippedActivityText, expectedStrippedName);
        }

        [TestMethod]
        public void RemoveRecipientMention_forNonTeamsScenario()
        {
            var activity = CreateActivity();
            activity.Text = "<at>firstName</at> lastName\n";
            var expectedStrippedName = "lastName";

            var mention = new Mention
            {
                Mentioned = new ChannelAccount()
                {
                    Id = activity.Recipient.Id,
                    Name = "<at>firstName</at>",
                },
                Text = "<at>firstName</at>",
                Type = "mention",
            };
            var lst = new List<Entity>();

            var output = JsonConvert.SerializeObject(mention);
            var entity = JsonConvert.DeserializeObject<Entity>(output);
            lst.Add(entity);
            activity.Entities = lst;

            var strippedActivityText = activity.RemoveRecipientMention();
            Assert.AreEqual(strippedActivityText, expectedStrippedName);
        }

        [TestMethod]
        public void ApplyConversationReference_isIncoming()
        {
            var activity = CreateActivity();
            var conversationReference = new ConversationReference
            {
                ChannelId = "cr_123",
                ServiceUrl = "cr_serviceUrl",
                Conversation = new ConversationAccount
                {
                    Id = "cr_456",
                },
                User = new ChannelAccount
                {
                    Id = "cr_abc",
                },
                Bot = new ChannelAccount
                {
                    Id = "cr_def",
                },
                ActivityId = "cr_12345",
                Locale = "en-uS" // Intentionally oddly-cased to check that it isn't defaulted somewhere, but tests stay in English
            };

            activity.ApplyConversationReference(conversationReference, true);

            Assert.AreEqual(conversationReference.ChannelId, activity.ChannelId);
            Assert.AreEqual(conversationReference.Locale, activity.Locale);
            Assert.AreEqual(conversationReference.ServiceUrl, activity.ServiceUrl);
            Assert.AreEqual(conversationReference.Conversation.Id, activity.Conversation.Id);

            Assert.AreEqual(conversationReference.User.Id, activity.From.Id);
            Assert.AreEqual(conversationReference.Bot.Id, activity.Recipient.Id);
            Assert.AreEqual(conversationReference.ActivityId, activity.Id);
        }

        [TestMethod]
        public void ApplyConversationReference()
        {
            var activity = CreateActivity();

            var conversationReference = new ConversationReference
            {
                ChannelId = "123",
                ServiceUrl = "serviceUrl",
                Conversation = new ConversationAccount
                {
                    Id = "456",
                },
                User = new ChannelAccount
                {
                    Id = "abc",
                },
                Bot = new ChannelAccount
                {
                    Id = "def",
                },
                ActivityId = "12345",
                Locale = "en-uS" // Intentionally oddly-cased to check that it isn't defaulted somewhere, but tests stay in English
            };

            activity.ApplyConversationReference(conversationReference, false);

            Assert.AreEqual(conversationReference.ChannelId, activity.ChannelId);
            Assert.AreEqual(conversationReference.Locale, activity.Locale);
            Assert.AreEqual(conversationReference.ServiceUrl, activity.ServiceUrl);
            Assert.AreEqual(conversationReference.Conversation.Id, activity.Conversation.Id);

            Assert.AreEqual(conversationReference.Bot.Id, activity.From.Id);
            Assert.AreEqual(conversationReference.User.Id, activity.Recipient.Id);
            Assert.AreEqual(conversationReference.ActivityId, activity.ReplyToId);
        }

        [TestMethod]
        public void CreateTraceAllowsNullRecipient()
        {
            // https://github.com/Microsoft/botbuilder-dotnet/issues/1580
            var activity = CreateActivity();
            activity.Recipient = null;
            var trace = activity.CreateTrace("test");

            // CreateTrace flips Recipient and From
            Assert.IsNull(trace.From.Id);
        }

        [TestMethod]
        public void IsFromStreamingConnectionTests()
        {
            var nonStreaming = new List<string>()
            {
                "http://yayay.com",
                "https://yayay.com",
                "HTTP://yayay.com",
                "HTTPS://yayay.com",
            };

            var streaming = new List<string>()
            {
                "urn:botframework:WebSocket:wss://beep.com",
                "urn:botframework:WebSocket:http://beep.com",
                "URN:botframework:WebSocket:wss://beep.com",
                "URN:botframework:WebSocket:http://beep.com",
            };

            var activity = CreateActivity();

            nonStreaming.ForEach(s =>
            {
                activity.ServiceUrl = s;
                Assert.IsFalse(activity.IsFromStreamingConnection());
            });

            streaming.ForEach(s =>
            {
                activity.ServiceUrl = s;
                Assert.IsTrue(activity.IsFromStreamingConnection());
            });
        }

        private Activity CreateActivity()
        {
            var account1 = new ChannelAccount
            {
                Id = "ChannelAccount_Id_1",
                Name = "ChannelAccount_Name_1",
                Properties = new JObject { { "Name", "Value" } },
                Role = "ChannelAccount_Role_1",
            };

            var account2 = new ChannelAccount
            {
                Id = "ChannelAccount_Id_2",
                Name = "ChannelAccount_Name_2",
                Properties = new JObject { { "Name", "Value" } },
                Role = "ChannelAccount_Role_2",
            };

            var conversationAccount = new ConversationAccount
            {
                ConversationType = "a",
                Id = "123",
                IsGroup = true,
                Name = "Name",
                Properties = new JObject { { "Name", "Value" } },
                Role = "ConversationAccount_Role",
            };

            var activity = new Activity
            {
                Id = "123",
                From = account1,
                Recipient = account2,
                Conversation = conversationAccount,
                ChannelId = "ChannelId123",
                Locale = "en-uS", // Intentionally oddly-cased to check that it isn't defaulted somewhere, but tests stay in English
                ServiceUrl = "ServiceUrl123",
            };

            return activity;
        }
    }
}
