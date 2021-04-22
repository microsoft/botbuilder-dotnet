// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class ActivityTest
    {
        [Fact]
        public void GetConversationReference()
        {
            var activity = CreateActivity("en-us");

            var conversationReference = activity.GetConversationReference();

            Assert.Equal(activity.Id, conversationReference.ActivityId);
            Assert.Equal(activity.From.Id, conversationReference.User.Id);
            Assert.Equal(activity.Recipient.Id, conversationReference.Bot.Id);
            Assert.Equal(activity.Conversation.Id, conversationReference.Conversation.Id);
            Assert.Equal(activity.ChannelId, conversationReference.ChannelId);
            Assert.Equal(activity.Locale, conversationReference.Locale);
            Assert.Equal(activity.ServiceUrl, conversationReference.ServiceUrl);
        }

        [Fact]
        public void GetReplyConversationReference()
        {
            var activity = CreateActivity("en-us");

            var reply = new ResourceResponse
            {
                Id = "1234",
            };

            var conversationReference = activity.GetReplyConversationReference(reply);

            Assert.Equal(reply.Id, conversationReference.ActivityId);
            Assert.Equal(activity.From.Id, conversationReference.User.Id);
            Assert.Equal(activity.Recipient.Id, conversationReference.Bot.Id);
            Assert.Equal(activity.Conversation.Id, conversationReference.Conversation.Id);
            Assert.Equal(activity.ChannelId, conversationReference.ChannelId);
            Assert.Equal(activity.Locale, conversationReference.Locale);
            Assert.Equal(activity.ServiceUrl, conversationReference.ServiceUrl);
        }

        [Fact]
        public void RemoveRecipientMention_forTeams()
        {
            var activity = CreateActivity("en-us");
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
            Assert.Equal(strippedActivityText, expectedStrippedName);
        }

        [Fact]
        public void RemoveRecipientMention_forNonTeamsScenario()
        {
            var activity = CreateActivity("en-us");
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
            Assert.Equal(strippedActivityText, expectedStrippedName);
        }

        [Fact]
        public void ApplyConversationReference_isIncoming()
        {
            var activity = CreateActivity("en-us");
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

            Assert.Equal(conversationReference.ChannelId, activity.ChannelId);
            Assert.Equal(conversationReference.Locale, activity.Locale);
            Assert.Equal(conversationReference.ServiceUrl, activity.ServiceUrl);
            Assert.Equal(conversationReference.Conversation.Id, activity.Conversation.Id);

            Assert.Equal(conversationReference.User.Id, activity.From.Id);
            Assert.Equal(conversationReference.Bot.Id, activity.Recipient.Id);
            Assert.Equal(conversationReference.ActivityId, activity.Id);
        }

        [Theory]
        [InlineData("en-uS")] // Intentionally oddly-cased to check that it isn't defaulted somewhere, but tests stay in English
        [InlineData(null)]
        public void ApplyConversationReference(string convoRefLocale)
        {
            var activity = CreateActivity("en-us");

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
                Locale = convoRefLocale
            };

            activity.ApplyConversationReference(conversationReference, false);

            Assert.Equal(conversationReference.ChannelId, activity.ChannelId);
            Assert.Equal(conversationReference.ServiceUrl, activity.ServiceUrl);
            Assert.Equal(conversationReference.Conversation.Id, activity.Conversation.Id);

            Assert.Equal(conversationReference.Bot.Id, activity.From.Id);
            Assert.Equal(conversationReference.User.Id, activity.Recipient.Id);
            Assert.Equal(conversationReference.ActivityId, activity.ReplyToId);

            if (convoRefLocale == null)
            {
                Assert.NotEqual(conversationReference.Locale, activity.Locale);
            }
            else
            {
                Assert.Equal(conversationReference.Locale, activity.Locale);
            }
        }

        [Fact]
        public void CreateTraceAllowsNullRecipient()
        {
            // https://github.com/Microsoft/botbuilder-dotnet/issues/1580
            var activity = CreateActivity("en-us");
            activity.Recipient = null;
            var trace = activity.CreateTrace("test");

            // CreateTrace flips Recipient and From
            Assert.Null(trace.From.Id);
        }

        [Fact]
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

            var activity = CreateActivity("en-uS");

            nonStreaming.ForEach(s =>
            {
                activity.ServiceUrl = s;
                Assert.False(activity.IsFromStreamingConnection());
            });

            streaming.ForEach(s =>
            {
                activity.ServiceUrl = s;
                Assert.True(activity.IsFromStreamingConnection());
            });
        }

        [Theory]
        [InlineData(nameof(ActivityTypes.ContactRelationUpdate))]
        [InlineData(nameof(ActivityTypes.EndOfConversation))]
        [InlineData(nameof(ActivityTypes.Event))]
        [InlineData(nameof(ActivityTypes.Handoff))]
        [InlineData(nameof(ActivityTypes.Invoke))]
        [InlineData(nameof(ActivityTypes.Message))]
        [InlineData(nameof(ActivityTypes.Typing))]
        public void CanCreateActivities(string activityType)
        {
            var createActivityMethod = typeof(Activity).GetMethod($"Create{activityType}Activity");
            var activity = (Activity)createActivityMethod.Invoke(null, new object[0]);
            var expectedActivityType = (string)typeof(ActivityTypes).GetField(activityType).GetValue(null);

            Assert.NotNull(activity);
            Assert.True(activity.Type == expectedActivityType);

            if (expectedActivityType == ActivityTypes.Message)
            {
                Assert.IsType<List<Attachment>>(activity.Attachments);
                Assert.True(activity.Attachments.Count == 0);
                Assert.IsType<List<Entity>>(activity.Entities);
            }
        }

        [Theory]
        [InlineData("TestTrace", null, null, null)]
        [InlineData("TestTrace", null, "TestValue", null)]
        public void CanCreateTraceActivity(string name, string valueType, object value, string label)
        {
            var activity = Activity.CreateTraceActivity(name, valueType, value, label);

            Assert.NotNull(activity);
            Assert.True(activity.Type == ActivityTypes.Trace);
            Assert.True(activity.Name == name);
            Assert.True(activity.ValueType == value?.GetType().Name);
            Assert.True(activity.Value == value);
            Assert.True(activity.Label == label);
        }

        [Theory]
        [InlineData("en-uS", "response")] // Default locale intentionally oddly-cased to check that it isn't defaulted somewhere, but tests stay in English
        [InlineData("en-uS", "response", false)] 
        [InlineData(null, "")]
        public void CanCreateReplyActivity(string locale, string text, bool createRecipient = true)
        {
            var activity = CreateActivity(locale, createRecipient);
            var reply = activity.CreateReply(text);

            Assert.NotNull(reply);
            Assert.True(reply.Type == ActivityTypes.Message);
            if (createRecipient == true)
            {
                Assert.True(reply.From.Id == "ChannelAccount_Id_2");
                Assert.True(reply.From.Name == "ChannelAccount_Name_2");
            }
            else
            {
                Assert.Null(reply.From.Id);
                Assert.Null(reply.From.Name);
            }

            Assert.True(reply.Recipient.Id == "ChannelAccount_Id_1");
            Assert.True(reply.Recipient.Name == "ChannelAccount_Name_1");
            Assert.True(reply.ReplyToId == "123");
            Assert.True(reply.ServiceUrl == "ServiceUrl123");
            Assert.True(reply.ChannelId == "ChannelId123");
            Assert.True(reply.Conversation.IsGroup);
            Assert.True(reply.Text == text);
            Assert.True(reply.Locale == locale);
        }

        // Default locale intentionally oddly-cased to check that it isn't defaulted somewhere, but tests stay in English
        private static Activity CreateActivity(string locale, bool createRecipient = true)
        {
            var account1 = new ChannelAccount
            {
                Id = "ChannelAccount_Id_1",
                Name = "ChannelAccount_Name_1",
                Properties = new JObject { { "Name", "Value" } },
                Role = "ChannelAccount_Role_1",
            };

            var account2 = createRecipient ? new ChannelAccount
            {
                Id = "ChannelAccount_Id_2",
                Name = "ChannelAccount_Name_2",
                Properties = new JObject { { "Name", "Value" } },
                Role = "ChannelAccount_Role_2",
            }
            : null;

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
                Locale = locale,
                ServiceUrl = "ServiceUrl123",
            };

            return activity;
        }
    }
}
