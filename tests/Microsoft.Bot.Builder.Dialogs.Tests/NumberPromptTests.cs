// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Dialogs.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class NumberPromptTests
    {
        [TestMethod]
        public async Task NumberPrompt()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<Dictionary<string, object>>("test");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await testProperty.GetAsync(turnContext, () => new Dictionary<string, object>());
                var prompt = new NumberPrompt<int>(Culture.English);

                var dialogCompletion = await prompt.ContinueAsync(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.BeginAsync(turnContext, state, new PromptOptions { PromptString = "Enter a number." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<int>)dialogCompletion.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("42")
            .AssertReply("Bot received the number '42'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task NumberPromptRetry()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<Dictionary<string, object>>("test");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await testProperty.GetAsync(turnContext, () => new Dictionary<string, object>());
                var prompt = new NumberPrompt<int>(Culture.English);

                var dialogCompletion = await prompt.ContinueAsync(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.BeginAsync(turnContext, state,
                        new PromptOptions
                        {
                            PromptString = "Enter a number.",
                            RetryPromptString = "You must enter a number."
                        });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<int>)dialogCompletion.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("hello")
            .AssertReply("You must enter a number.")
            .Send("64")
            .AssertReply("Bot received the number '64'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task NumberPromptValidator()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<Dictionary<string, object>>("test");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);


                PromptValidator<NumberResult<int>> validator = async (ctx, result) =>
            {
                if (result.Value < 0)
                    result.Status = PromptStatus.TooSmall;
                if (result.Value > 100)
                    result.Status = PromptStatus.TooBig;
                await Task.CompletedTask;
            };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await testProperty.GetAsync(turnContext, () => new Dictionary<string, object>());
                var prompt = new NumberPrompt<int>(Culture.English, validator);

                var dialogCompletion = await prompt.ContinueAsync(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.BeginAsync(turnContext, state,
                        new PromptOptions
                        {
                            PromptString = "Enter a number.",
                            RetryPromptString = "You must enter a positive number less than 100."
                        });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<int>)dialogCompletion.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("150")
            .AssertReply("You must enter a positive number less than 100.")
            .Send("64")
            .AssertReply("Bot received the number '64'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task FloatNumberPrompt()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<Dictionary<string, object>>("test");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await testProperty.GetAsync(turnContext, () => new Dictionary<string, object>());
                var prompt = new NumberPrompt<float>(Culture.English);

                var dialogCompletion = await prompt.ContinueAsync(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.BeginAsync(turnContext, state, new PromptOptions { PromptString = "Enter a number." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<float>)dialogCompletion.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("3.14")
            .AssertReply("Bot received the number '3.14'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task LongNumberPrompt()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<Dictionary<string, object>>("test");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await testProperty.GetAsync(turnContext, () => new Dictionary<string, object>());
                var prompt = new NumberPrompt<long>(Culture.English);

                var dialogCompletion = await prompt.ContinueAsync(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.BeginAsync(turnContext, state, new PromptOptions { PromptString = "Enter a number." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<long>)dialogCompletion.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("42")
            .AssertReply("Bot received the number '42'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task DoubleNumberPrompt()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<Dictionary<string, object>>("test");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await testProperty.GetAsync(turnContext, () => new Dictionary<string, object>());
                var prompt = new NumberPrompt<double>(Culture.English);

                var dialogCompletion = await prompt.ContinueAsync(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.BeginAsync(turnContext, state, new PromptOptions { PromptString = "Enter a number." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<double>)dialogCompletion.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("3.14")
            .AssertReply("Bot received the number '3.14'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task DecimalNumberPrompt()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<Dictionary<string, object>>("test");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await testProperty.GetAsync(turnContext, () => new Dictionary<string, object>());
                var prompt = new NumberPrompt<decimal>(Culture.English);

                var dialogCompletion = await prompt.ContinueAsync(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.BeginAsync(turnContext, state, new PromptOptions { PromptString = "Enter a number." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<decimal>)dialogCompletion.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("3.14")
            .AssertReply("Bot received the number '3.14'.")
            .StartTestAsync();
        }
    }
}
