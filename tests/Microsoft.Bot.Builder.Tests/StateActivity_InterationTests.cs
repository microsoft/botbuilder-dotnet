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
    class StateActivity_InterationTests
    {

        [TestMethod]
        public void State_Conversation_DoNOTRememberConversationAfterEndOfConversation()
        {
            IList<IActivity> testActivities = InteractionTestsActivityHelper.StartNewConversation();

            TestAdapter adapter = new TestAdapter()
                .Use(new UserStateManagerMiddleware(new MemoryStorage()))
                .Use(new ConversationStateManagerMiddleware(new MemoryStorage()));

            var testFlow = new TestFlow(adapter, async (context) =>
            {
                Assert.IsNotNull(context.State, "context.state should exist");
                string text = context.Request.AsMessageActivity()?.Text.Trim().ToLower();
                if (text != null)
                {
                    var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var name = parts?[1] ?? "value";
                    var input = parts?[2] ?? "test";
                    switch (parts[0])
                    {
                        case "set":
                            context.State.ConversationProperties[name] = input;
                            context.Reply($"{input} saved to conversation state[{name}].");
                            break;
                        case "get":
                            string state = context.State.ConversationProperties[name];
                            context.Reply(state);
                            break;
                    }
                }
            });
            foreach (var activity in InteractionTestsActivityHelper.StartNewConversation())
            {
                testFlow.Send(activity);
            }
            //await testFlow
            //    .Test("get", string.Empty, "Conversation state should NOT be remembered after EndOfConversation.")
            //    .StartTest();
        }

        [TestMethod]
        public async void State_User_RememberUserAfterEndOfConversation()
        {
            IList<IActivity> testActivities = InteractionTestsActivityHelper.StartNewConversation();

            TestAdapter adapter = new TestAdapter()
                .Use(new UserStateManagerMiddleware(new MemoryStorage()))
                .Use(new ConversationStateManagerMiddleware(new MemoryStorage()));

            var testFlow = new TestFlow(adapter, async (context) =>
            {
                Assert.IsNotNull(context.State, "context.state should exist");
                string text = context.Request.AsMessageActivity()?.Text.Trim().ToLower();
                if (text != null)
                {
                    var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var name = parts?[1] ?? "value";
                    var input = parts?[2] ?? "test";
                    switch (parts[0])
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
            foreach (var activity in InteractionTestsActivityHelper.StartNewConversation())
            {
                testFlow.Send(activity);
            }
            await testFlow
                .Test("get", "test", "User state should be remembered after EndOfConversation.")
                .StartTest();
        }

        [TestMethod]
        public async void State_User_DoNOTRememberUserAfterDeleteUserData()
        {
            IList<IActivity> testActivities = InteractionTestsActivityHelper.StartNewConversation();

            TestAdapter adapter = new TestAdapter()
                .Use(new UserStateManagerMiddleware(new MemoryStorage()))
                .Use(new ConversationStateManagerMiddleware(new MemoryStorage()));

            var testFlow = new TestFlow(adapter, async (context) =>
            {
                Assert.IsNotNull(context.State, "context.state should exist");
                string text = context.Request.AsMessageActivity()?.Text.Trim().ToLower();
                if (text != null)
                {
                    var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var name = parts?[1] ?? "value";
                    var input = parts?[2] ?? "test";
                    switch (parts[0])
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
            foreach (var activity in InteractionTestsActivityHelper.StartNewConversation())
            {
                testFlow.Send(activity);
            }
            await testFlow
                .Test("get", string.Empty, "User state should NOT be remembered after DeleteUserData.")
                .StartTest();
        }
    }
}
