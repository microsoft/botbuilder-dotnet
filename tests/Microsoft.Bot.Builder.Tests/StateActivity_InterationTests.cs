using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("State Management")]
    public class StateActivity_InterationTests
    {
        [TestMethod]
        public async Task State_Conversation_DoNOTRememberConversationAfterEndOfConversation()
        {
            IList<IActivity> testActivities = StateActivity_Helper.StartNewConversation();

            TestAdapter adapter = new TestAdapter()
                .Use(new UserStateManagerMiddleware(new MemoryStorage()))
                .Use(new ConversationStateManagerMiddleware(new MemoryStorage()));

            var testFlow = new TestFlow(adapter, async (context) =>
            {
                Assert.IsNotNull(context.State, "context.state should exist");
                switch (context.Request.Type)
                {
                    case ActivityTypes.ConversationUpdate:
                    case ActivityTypes.EndOfConversation:
                    case ActivityTypes.DeleteUserData:
                    case ActivityTypes.Message:
                    default:
                        break;
                }
                string text = context.Request.AsMessageActivity()?.Text?.Trim().ToLower();
                    var name = "value";
                    var input = "test";
                    switch (text)
                    {
                        case "set":
                            context.State.ConversationProperties[name] = input;
                            context.Reply($"{input} saved to conversation state[{name}].");
                            break;
                        case "get":
                            string state = context.State.ConversationProperties[name];
                            context.Reply(state);
                            break;
                    default:
                        break;
                }
            });
            foreach (var activity in StateActivity_Helper.StartNewConversation())
            {
                testFlow.Send(activity);
            }
            await testFlow
                .Test("get", string.Empty, "Conversation state should NOT be remembered after EndOfConversation.")
                .StartTest();
        }

        [TestMethod]
        public async Task State_User_RememberUserAfterEndOfConversation()
        {
            IList<IActivity> testActivities = StateActivity_Helper.StartNewConversation();

            TestAdapter adapter = new TestAdapter()
                .Use(new UserStateManagerMiddleware(new MemoryStorage()))
                .Use(new ConversationStateManagerMiddleware(new MemoryStorage()));

            var testFlow = new TestFlow(adapter, async (context) =>
            {
                Assert.IsNotNull(context.State, "context.state should exist");
                string text = context.Request.AsMessageActivity()?.Text.Trim().ToLower();
                if (text != null)
                {
                    var name = "value";
                    var input = "test";
                    switch (text)
                    {
                        case "set":
                            context.State.UserProperties[name] = input;
                            context.Reply($"{input} saved to user state[{name}].");
                            break;
                        case "get":
                            string state = context.State.UserProperties[name];
                            context.Reply(state);
                            break;
                    }
                }
            });
            foreach (var activity in StateActivity_Helper.StartNewConversation())
            {
                testFlow.Send(activity);
            }
            await testFlow
                .Test("get", "test", "User state should be remembered after EndOfConversation.")
                .StartTest();
        }

        [TestMethod]
        public async Task State_User_DoNOTRememberUserAfterDeleteUserData()
        {
            IList<IActivity> testActivities = StateActivity_Helper.StartNewConversation();

            TestAdapter adapter = new TestAdapter()
                .Use(new UserStateManagerMiddleware(new MemoryStorage()))
                .Use(new ConversationStateManagerMiddleware(new MemoryStorage()));

            var testFlow = new TestFlow(adapter, async (context) =>
            {
                Assert.IsNotNull(context.State, "context.state should exist");
                string text = context.Request.AsMessageActivity()?.Text.Trim().ToLower();
                if (text != null)
                {
                    var name = "value";
                    var input = "test";
                    switch (text)
                    {
                        case "set":
                            context.State.UserProperties[name] = input;
                            context.Reply($"{input} saved to user state[{name}].");
                            break;
                        case "get":
                            string state = context.State.UserProperties[name];
                            context.Reply(state);
                            break;
                    }
                }
            });
            foreach (var activity in StateActivity_Helper.StartNewConversation())
            {
                testFlow.Send(activity);
            }
            await testFlow
                .Test("get", string.Empty, "User state should NOT be remembered after DeleteUserData.")
                .StartTest();
        }
    }

    public static class StateActivity_Helper
    {
        public static ConversationReference CreateConversationReference(
            string userName = "User1", string conversationName = "Convo1")
        {
            var conversationReference = new ConversationReference
            {
                ChannelId = "test",
                ServiceUrl = "https://test.com"
            };
            conversationReference.User = new ChannelAccount(userName.ToLower(), userName);
            conversationReference.Bot = new ChannelAccount("bot", "Bot");
            conversationReference.Conversation = new ConversationAccount(
                false, conversationName.ToLower(), conversationName);
            return conversationReference;
        }

        public static IEnumerable<IActivity> StartConversation(ConversationReference reference)
        {
            yield return new Activity()
            {
                Type = ActivityTypes.ConversationUpdate,
                Conversation = reference.Conversation,
                From = reference.User,
                Recipient = reference.Bot,
                ServiceUrl = reference.ServiceUrl,
                MembersAdded = new List<ChannelAccount> { reference.User },
                MembersRemoved = new List<ChannelAccount>()
            };
            yield return new Activity()
            {
                Type = ActivityTypes.ConversationUpdate,
                Conversation = reference.Conversation,
                From = reference.User,
                Recipient = reference.Bot,
                ServiceUrl = reference.ServiceUrl,
                MembersAdded = new List<ChannelAccount> { reference.Bot },
                MembersRemoved = new List<ChannelAccount>()
            };
        }

        public static IActivity CreateMessageActivity(ConversationReference reference, string message)
        {
            return new Activity()
            {
                Type = ActivityTypes.Message,
                Conversation = reference.Conversation,
                From = reference.User,
                Recipient = reference.Bot,
                ServiceUrl = reference.ServiceUrl,
                Text = message
            };
        }

        public static IActivity CreateEndOfConversation(ConversationReference reference, string endOfConversationCode = EndOfConversationCodes.Unknown)
        {
            return new Activity()
            {
                Type = ActivityTypes.EndOfConversation,
                Conversation = reference.Conversation,
                From = reference.User,
                Recipient = reference.Bot,
                ServiceUrl = reference.ServiceUrl,
                Code = endOfConversationCode
            };
        }

        public static IActivity CreateDeleteUserData(ConversationReference reference)
        {
            return new Activity()
            {
                Type = ActivityTypes.DeleteUserData,
                Conversation = reference.Conversation,
                From = reference.User,
                Recipient = reference.Bot,
                ServiceUrl = reference.ServiceUrl
            };
        }

        public static IList<IActivity> StartNewConversation()
        {
            var convo1 = CreateConversationReference(conversationName: "Convo1");
            var convo2 = CreateConversationReference(conversationName: "Convo2");
            var userAccount = convo1.User;
            var botAccount = convo1.Bot;

            var activities = new List<IActivity>();
            activities.AddRange(StartConversation(convo1));
            activities.Add(CreateMessageActivity(convo1, "set"));
            activities.Add(CreateEndOfConversation(convo1));
            activities.AddRange(StartConversation(convo2));

            return activities;
        }

        public static IList<IActivity> SetUpThenSendDeleteUserData()
        {
            var convo1 = CreateConversationReference(conversationName: "Convo1");
            var convo2 = CreateConversationReference(conversationName: "Convo2");
            var userAccount = convo1.User;
            var botAccount = convo1.Bot;

            var activities = new List<IActivity>();
            activities.AddRange(StartConversation(convo1));
            activities.Add(CreateMessageActivity(convo1, "set"));
            activities.Add(CreateDeleteUserData(convo1));

            return activities;
        }
    }
}
