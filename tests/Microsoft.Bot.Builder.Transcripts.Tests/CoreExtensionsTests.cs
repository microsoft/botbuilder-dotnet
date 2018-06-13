// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Transcripts.Tests
{
    [TestClass]
    public class CoreExtensionsTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task UserStateTest()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var storage = new MemoryStorage();

            TestAdapter adapter = new TestAdapter()
                .Use(new UserState<UserStateObject>(storage));

            var flow = new TestFlow(adapter, async (context) => {
                var (command, value) = GetCommandValue(context);
                switch (command)
                {
                    case "delete":
                        context.GetUserState<UserStateObject>().Value = null;
                        break;
                    case "set":
                        context.GetUserState<UserStateObject>().Value = value;
                        break;
                    case "read":
                        await context.SendActivity($"value:{context.GetUserState<UserStateObject>().Value}");
                        break;
                    default:
                        await context.SendActivity("bot message");
                        break;
                }
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task ConversationStateTest()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var storage = new MemoryStorage();

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<ConversationStateObject>(storage));

            var flow = new TestFlow(adapter, async (context) => {
                var (command, value) = GetCommandValue(context);
                switch (command)
                {
                    case "delete":
                        context.GetConversationState<ConversationStateObject>().Value = null;
                        break;
                    case "set":
                        context.GetConversationState<ConversationStateObject>().Value = value;
                        break;
                    case "read":
                        await context.SendActivity($"value:{context.GetConversationState<ConversationStateObject>().Value}");
                        break;
                    default:
                        await context.SendActivity("bot message");
                        break;
                }
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task CustomStateTest()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var storage = new MemoryStorage();

            TestAdapter adapter = new TestAdapter()
                .Use(new CustomState(storage));

            var flow = new TestFlow(adapter, async (context) => {
                var (command, value) = GetCommandValue(context);
                switch (command)
                {
                    case "delete":
                        CustomState.Get(context).Value = null;
                        break;
                    case "set":
                        CustomState.Get(context).Value = value;
                        break;
                    case "read":
                        await context.SendActivity($"value:{CustomState.Get(context).Value}");
                        break;
                    default:
                        await context.SendActivity("bot message");
                        break;
                }
            });

            await flow.Test(activities).StartTest();
        }

        private (string command, string value) GetCommandValue(ITurnContext context)
        {
            var message = context.Activity.Text.Split(' ');
            if (message.Length > 1)
            {
                return (message[0], message[1]);
            }
            return (message[0], null);
        }

        internal class UserStateObject
        {
            public string Value { get; set; }
        }

        internal class ConversationStateObject
        {
            public string Value { get; set; }
        }

        internal class CustomStateObject
        {
            public string Value { get; set; }
        }

        internal class CustomState : BotState<CustomStateObject>
        {
            public const string PropertyName = "Microsoft.Bot.Builder.Transcripts.Tests.CustomState";

            public CustomState(IStorage storage) : base(storage, PropertyName, (context) => "CustomKey")
            {
            }

            public static CustomStateObject Get(ITurnContext context) => context.Services.Get<CustomStateObject>(PropertyName);
        }

    }
}
