// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using static Microsoft.Bot.Schema.Tests.ActivityTestData;

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

        [Theory]
        [InlineData("myValue", null, false, false, null)]
        [InlineData(null, null, false, false, null)]
        [InlineData(null, "myValueType", false, false, null)]
        [InlineData(null, null, true, false, null)]
        [InlineData(null, null, false, true, "testLabel")]
        public void CreateTrace(string value, string valueType, bool createRecipient, bool createFrom, string label = null)
        {
            // https://github.com/Microsoft/botbuilder-dotnet/issues/1580
            var activity = CreateActivity("en-us", createRecipient, createFrom);
            var trace = activity.CreateTrace("test", value, valueType, label);

            Assert.NotNull(trace);
            Assert.True(trace.Type == ActivityTypes.Trace);
            Assert.True(trace.ValueType == (valueType ?? value?.GetType().Name));
            Assert.True(trace.Label == label);
            Assert.True(trace.Name == "test");
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

            var activity = CreateActivity("en-us");

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

            activity.ServiceUrl = null;
            Assert.False(activity.IsFromStreamingConnection());
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
        public void TestCreateTraceActivity(string name, string valueType, object value, string label)
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
        [InlineData("en-uS", "response", false, true, null)] // Default locale intentionally oddly-cased to check that it isn't defaulted somewhere, but tests stay in English
        [InlineData("en-uS", "response", false, false, null)]
        [InlineData(null, "", true, false, "en-us")]
        [InlineData(null, null, true, true, null)]
        public void CanCreateReplyActivity(string activityLocale, string text, bool createRecipient = true, bool createFrom = true, string createReplyLocale = null)
        {
            var activity = CreateActivity(activityLocale, createRecipient, createFrom);
            var reply = activity.CreateReply(text, createReplyLocale);

            Assert.NotNull(reply);
            Assert.True(reply.Type == ActivityTypes.Message);
            Assert.True(reply.ReplyToId == "123");
            Assert.True(reply.ServiceUrl == "ServiceUrl123");
            Assert.True(reply.ChannelId == "ChannelId123");
            Assert.True(reply.Conversation.IsGroup);
            Assert.True(reply.Text == (text ?? string.Empty));
            Assert.True(reply.Locale == (activityLocale ?? createReplyLocale));
            ValidateRecipientAndFrom(reply, createRecipient, createFrom);
        }

        [Theory]
        [InlineData(nameof(ActivityTypes.Command))]
        [InlineData(nameof(ActivityTypes.CommandResult))]
        [InlineData(nameof(ActivityTypes.ContactRelationUpdate))]
        [InlineData(nameof(ActivityTypes.ConversationUpdate))]
        [InlineData(nameof(ActivityTypes.EndOfConversation))]
        [InlineData(nameof(ActivityTypes.Event))]
        [InlineData(nameof(ActivityTypes.Handoff))]
        [InlineData(nameof(ActivityTypes.InstallationUpdate))]
        [InlineData(nameof(ActivityTypes.Invoke))]
        [InlineData(nameof(ActivityTypes.Message))]
        [InlineData(nameof(ActivityTypes.MessageDelete))]
        [InlineData(nameof(ActivityTypes.MessageReaction))]
        [InlineData(nameof(ActivityTypes.MessageUpdate))]
        [InlineData(nameof(ActivityTypes.Suggestion))]
        [InlineData(nameof(ActivityTypes.Typing))]
        public void CanCastToActivityType(string activityType)
        {
            var activity = new Activity()
            {
                Type = GetActivityType(activityType)
            };

            // This will return null if casting was unsuccessful, otherwise it should return an Activity
            var castActivity = CastToActivityType(activityType, activity);

            Assert.NotNull(activity);
            Assert.NotNull(castActivity);
            Assert.True(activity.Type.ToLowerInvariant() == activityType.ToLowerInvariant());
        }

        [Theory]
        [InlineData(nameof(ActivityTypes.Command))]
        [InlineData(nameof(ActivityTypes.CommandResult))]
        [InlineData(nameof(ActivityTypes.ContactRelationUpdate))]
        [InlineData(nameof(ActivityTypes.ConversationUpdate))]
        [InlineData(nameof(ActivityTypes.EndOfConversation))]
        [InlineData(nameof(ActivityTypes.Event))]
        [InlineData(nameof(ActivityTypes.Handoff))]
        [InlineData(nameof(ActivityTypes.InstallationUpdate))]
        [InlineData(nameof(ActivityTypes.Invoke))]
        [InlineData(nameof(ActivityTypes.Message))]
        [InlineData(nameof(ActivityTypes.MessageDelete))]
        [InlineData(nameof(ActivityTypes.MessageReaction))]
        [InlineData(nameof(ActivityTypes.MessageUpdate))]
        [InlineData(nameof(ActivityTypes.Suggestion))]
        [InlineData(nameof(ActivityTypes.Trace))]
        [InlineData(nameof(ActivityTypes.Typing))]
        public void CastToActivityType_ReturnNullsWhenCastUnsuccessful(string activityType)
        {
            var activity = new Activity();

            // This will return null if casting was unsuccessful, otherwise it should return an Activity
            var result = CastToActivityType(activityType, activity);

            Assert.NotNull(activity);
            Assert.Null(activity.Type);
            Assert.Null(result);
        }

        [Theory]
        [ClassData(typeof(TestChannelData))]
        public void GetChannelData(object channelData)
        {
            var activity = new Activity()
            {
                ChannelData = channelData
            };

            try
            {
                var result = activity.GetChannelData<MyChannelData>();
                if (channelData == null)
                {
                    Assert.Null(result);
                }
                else
                {
                    Assert.IsType<MyChannelData>(result);
                }
            }
            catch
            {
                Assert.IsNotType<MyChannelData>(channelData);
            }
        }

        [Fact]
        public void ShouldGetEmptyArrayIfNoMentions()
        {
            var activity = new Activity()
            {
                Type = ActivityTypes.Message,
            };

            var mentions = activity.GetMentions();

            Assert.IsType<Mention[]>(mentions);
            Assert.True(mentions.Length == 0);
        }

        [Theory]
        [ClassData(typeof(HasContentData))]
        public void HasContent(Activity activity, bool expected)
        {
            var hasContent = activity.HasContent();

            Assert.Equal(expected, hasContent);
        }

        [Theory]
        [InlineData("message/testType", ActivityTypes.Message, true)]
        [InlineData("message-testType", ActivityTypes.Message, false)]
        public void IsActivity(string typeOfActivity, string targetType, bool expected)
        {
            var activity = new TestActivity()
            {
                Type = typeOfActivity
            };

            Assert.Equal(expected, activity.IsTargetActivityType(targetType));
        }

        [Theory]
        [ClassData(typeof(TestChannelData))]
        public void TryGetChannelData(object channelData)
        {
            var activity = new Activity()
            {
                ChannelData = channelData
            };

            var successfullyGotChannelData = activity.TryGetChannelData(out MyChannelData data);
            var expectedSuccess = GetExpectedTryGetChannelDataResult(channelData);

            Assert.Equal(expectedSuccess, successfullyGotChannelData);
            if (successfullyGotChannelData == true)
            {
                Assert.NotNull(data);
                Assert.IsType<MyChannelData>(data);
            }
            else
            {
                Assert.Null(data);
            }
        }

        [Fact]
        public void CanSetCallerId()
        {
            var expectedCallerId = "callerId";
            var activity = new Activity()
            {
                CallerId = expectedCallerId
            };

            Assert.Equal(expectedCallerId, activity.CallerId);
        }

        [Fact]
        public void CanSetProperties()
        {
            var activity = new Activity()
            {
                Properties = new JObject()
            };

            var props = activity.Properties;
            Assert.NotNull(props);
            Assert.IsType<JObject>(props);
        }

        // Default locale intentionally oddly-cased to check that it isn't defaulted somewhere, but tests stay in English
        private static Activity CreateActivity(string locale, bool createRecipient = true, bool createFrom = true)
        {
            var account1 = createFrom ? new ChannelAccount
            {
                Id = "ChannelAccount_Id_1",
                Name = "ChannelAccount_Name_1",
                Properties = new JObject { { "Name", "Value" } },
                Role = "ChannelAccount_Role_1",
            }
            : null;

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

        private string GetActivityType(string type)
        {
            return (string)typeof(ActivityTypes).GetField(type).GetValue(null);
        }

        private Activity CastToActivityType(string activityType, IActivity activity)
        {
            var castMethod = typeof(Activity).GetMethod($"As{activityType}Activity");
            return (Activity)castMethod.Invoke(activity, new object[0]);
        }

        private void ValidateRecipientAndFrom(IActivity activity, bool createRecipient, bool createFrom)
        {
            if (createRecipient == true)
            {
                Assert.True(activity.From.Id == "ChannelAccount_Id_2");
                Assert.True(activity.From.Name == "ChannelAccount_Name_2");
            }
            else
            {
                Assert.Null(activity.From.Id);
                Assert.Null(activity.From.Name);
            }

            if (createFrom == true)
            {
                Assert.True(activity.Recipient.Id == "ChannelAccount_Id_1");
                Assert.True(activity.Recipient.Name == "ChannelAccount_Name_1");
            }
            else
            {
                Assert.Null(activity.Recipient.Id);
                Assert.Null(activity.Recipient.Name);
            }
        }

        private bool GetExpectedTryGetChannelDataResult(object channelData)
        {
            return channelData?.GetType() == typeof(JObject) || channelData?.GetType() == typeof(MyChannelData);
        }
    }
}
