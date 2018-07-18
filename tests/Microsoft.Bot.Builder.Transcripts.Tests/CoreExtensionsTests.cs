// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
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

            var userState = new UserState(new MemoryStorage());
            var testProperty = userState.CreateProperty<UserStateObject>("test", () => new UserStateObject());

            TestAdapter adapter = new TestAdapter()
                .Use(userState);

            var flow = new TestFlow(adapter, async (context) =>
            {
                if (context.Activity.Type == ActivityTypes.Message)
                {
                    var (command, value) = GetCommandValue(context);
                    switch (command)
                    {
                        case "delete":
                            await testProperty.DeleteAsync(context);
                            break;
                        case "set":
                            {
                                var data = await testProperty.GetAsync(context);
                                data.Value = value;
                                await testProperty.SetAsync(context, data);
                            }
                            break;
                        case "read":
                            {
                                var data = await testProperty.GetAsync(context);
                                await context.SendActivityAsync($"value:{data.Value}");
                            }
                            break;
                        default:
                            await context.SendActivityAsync("bot message");
                            break;
                    }
                }
            });

            await flow.Test(activities).StartTestAsync();
        }

        [TestMethod]
        public async Task ConversationStateTest()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var storage = new MemoryStorage();

            var convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<ConversationStateObject>("test", () => new ConversationStateObject());

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            var flow = new TestFlow(adapter, async (context) =>
            {
                if (context.Activity.Type == ActivityTypes.Message)
                {
                    var (command, value) = GetCommandValue(context);
                    switch (command)
                    {
                        case "delete":
                            await testProperty.DeleteAsync(context);
                            break;
                        case "set":
                            {
                                var data = await testProperty.GetAsync(context);
                                data.Value = value;
                                await testProperty.SetAsync(context, data);
                            }
                            break;
                        case "read":
                            {
                                var data = await testProperty.GetAsync(context);
                                await context.SendActivityAsync($"value:{data.Value}");
                            }
                            break;
                        default:
                            await context.SendActivityAsync("bot message");
                            break;
                    }
                }
            });

            await flow.Test(activities).StartTestAsync();
        }

        [TestMethod]
        public async Task CustomStateTest()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var storage = new MemoryStorage();
            var customState = new CustomState(storage);
            var testProperty = customState.CreateProperty<CustomStateObject>("Test", () => new CustomStateObject());
            TestAdapter adapter = new TestAdapter()
                .Use(customState);

            var flow = new TestFlow(adapter, async (context) =>
            {
                if (context.Activity.Type == ActivityTypes.Message)
                {
                    var (command, value) = GetCommandValue(context);
                    switch (command)
                    {
                        case "delete":
                            await testProperty.DeleteAsync(context);
                            break;
                        case "set":
                            {
                                var data = await testProperty.GetAsync(context);
                                data.Value = value;
                                await testProperty.SetAsync(context, data);
                            }
                            break;
                        case "read":
                            {
                                var data = await testProperty.GetAsync(context);
                                await context.SendActivityAsync($"value:{data.Value}");
                            }
                            break;
                        default:
                            await context.SendActivityAsync("bot message");
                            break;
                    }
                }
            });

            await flow.Test(activities).StartTestAsync();
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

        internal class CustomState : BotState
        {
            public const string PropertyName = "Microsoft.Bot.Builder.Transcripts.Tests.CustomState";

            public CustomState(IStorage storage) : base(storage, PropertyName, (context) => "CustomKey")
            {
            }

        }

    }
}
