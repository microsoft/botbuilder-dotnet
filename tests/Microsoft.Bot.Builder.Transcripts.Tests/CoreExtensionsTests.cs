// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Transcripts.Tests
{
    public class CoreExtensionsTests
    {
        public static readonly string ClassName = "CoreExtensionsTests";

        [Fact]
        public async Task UserStateTest()
        {
            var testName = "UserStateTest";
            var activities = TranscriptUtilities.GetActivitiesFromFile(ClassName, testName);

            var userState = new UserState(new MemoryStorage());
            var testProperty = userState.CreateProperty<UserStateObject>("test");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(testName))
                .Use(new AutoSaveStateMiddleware(userState));

            var flow = new TestFlow(adapter, async (context, cancellationToken) =>
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
                                var data = await testProperty.GetAsync(context, () => new UserStateObject());
                                data.Value = value;
                                await testProperty.SetAsync(context, data);
                            }

                            break;
                        case "read":
                            {
                                var data = await testProperty.GetAsync(context, () => new UserStateObject());
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

        [Fact]
        public async Task ConversationStateTest()
        {
            var testName = "ConversationStateTest";
            var activities = TranscriptUtilities.GetActivitiesFromFile(ClassName, testName);

            var storage = new MemoryStorage();

            var convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<ConversationStateObject>("test");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(testName))
                .Use(new AutoSaveStateMiddleware(convoState));

            var flow = new TestFlow(adapter, async (context, cancellationToken) =>
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
                                var data = await testProperty.GetAsync(context, () => new ConversationStateObject());
                                data.Value = value;
                                await testProperty.SetAsync(context, data);
                            }

                            break;
                        case "read":
                            {
                                var data = await testProperty.GetAsync(context, () => new ConversationStateObject());
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

        [Fact]
        public async Task CustomStateTest()
        {
            var testName = "CustomStateTest";
            var activities = TranscriptUtilities.GetActivitiesFromFile(ClassName, testName);

            var storage = new MemoryStorage();
            var customState = new CustomState(storage);
            var testProperty = customState.CreateProperty<CustomStateObject>("Test");
            var adapter = new TestAdapter(TestAdapter.CreateConversation(testName))
                .Use(new AutoSaveStateMiddleware(customState));

            var flow = new TestFlow(adapter, async (context, cancellationToken) =>
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
                                var data = await testProperty.GetAsync(context, () => new CustomStateObject());
                                data.Value = value;
                                await testProperty.SetAsync(context, data);
                            }

                            break;
                        case "read":
                            {
                                var data = await testProperty.GetAsync(context, () => new CustomStateObject());
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

            public CustomState(IStorage storage)
                : base(storage, PropertyName)
            {
            }

            protected override string GetStorageKey(ITurnContext turnContext) => "CustomKey";
        }
    }
}
