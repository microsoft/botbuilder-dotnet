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
            var testProperty = userState.CreateProperty<UserStateObject>("test");

            var adapter = new TestAdapter();

            var flow = new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await userState.LoadAsync(turnContext);
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    var (command, value) = GetCommandValue(turnContext);
                    switch (command)
                    {
                        case "delete":
                            await testProperty.DeleteAsync(turnContext);
                            break;
                        case "set":
                            {
                                var data = await testProperty.GetAsync(turnContext, () => new UserStateObject());
                                data.Value = value;
                                await testProperty.SetAsync(turnContext, data);
                            }
                            break;
                        case "read":
                            {
                                var data = await testProperty.GetAsync(turnContext, () => new UserStateObject());
                                await turnContext.SendActivityAsync($"value:{data.Value}");
                            }
                            break;
                        default:
                            await turnContext.SendActivityAsync("bot message");
                            break;
                    }
                }
                await userState.SaveChangesAsync(turnContext);
            });

            await flow.Test(activities).StartTestAsync();
        }

        [TestMethod]
        public async Task ConversationStateTest()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var storage = new MemoryStorage();

            var convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<ConversationStateObject>("test");

            var adapter = new TestAdapter();

            var flow = new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await convoState.LoadAsync(turnContext);
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    var (command, value) = GetCommandValue(turnContext);
                    switch (command)
                    {
                        case "delete":
                            await testProperty.DeleteAsync(turnContext);
                            break;
                        case "set":
                            {
                                var data = await testProperty.GetAsync(turnContext, () => new ConversationStateObject());
                                data.Value = value;
                                await testProperty.SetAsync(turnContext, data);
                            }
                            break;
                        case "read":
                            {
                                var data = await testProperty.GetAsync(turnContext, () => new ConversationStateObject());
                                await turnContext.SendActivityAsync($"value:{data.Value}");
                            }
                            break;
                        default:
                            await turnContext.SendActivityAsync("bot message");
                            break;
                    }
                }
                await convoState.SaveChangesAsync(turnContext);
            });

            await flow.Test(activities).StartTestAsync();
        }

        [TestMethod]
        public async Task CustomStateTest()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var storage = new MemoryStorage();
            var customState = new CustomState(storage);
            var testProperty = customState.CreateProperty<CustomStateObject>("Test");
            var adapter = new TestAdapter();

            var flow = new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await customState.LoadAsync(turnContext);
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    var (command, value) = GetCommandValue(turnContext);
                    switch (command)
                    {
                        case "delete":
                            await testProperty.DeleteAsync(turnContext);
                            break;
                        case "set":
                            {
                                var data = await testProperty.GetAsync(turnContext, () => new CustomStateObject());
                                data.Value = value;
                                await testProperty.SetAsync(turnContext, data);
                            }
                            break;
                        case "read":
                            {
                                var data = await testProperty.GetAsync(turnContext, () => new CustomStateObject());
                                await turnContext.SendActivityAsync($"value:{data.Value}");
                            }
                            break;
                        default:
                            await turnContext.SendActivityAsync("bot message");
                            break;
                    }
                }
                await customState.SaveChangesAsync(turnContext);
            });

            await flow.Test(activities).StartTestAsync();
        }

        private (string command, string value) GetCommandValue(ITurnContext turnContext)
        {
            var message = turnContext.Activity.Text.Split(' ');
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

            public CustomState(IStorage storage) : base(storage, PropertyName)
            {
            }

            protected override string GetStorageKey(ITurnContext turnContext) => "CustomKey";
        }
    }
}
