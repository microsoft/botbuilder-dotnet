// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class NumberPromptTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NumberPromptWithEmptyIdShouldFail()
        {
            var emptyId = "";
            var numberPrompt = new NumberPrompt<int>(emptyId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NumberPromptWithNullIdShouldFail()
        {
            var nullId = "";
            nullId = null;
            var numberPrompt = new NumberPrompt<int>(nullId);
        }

        [TestMethod]
        public async Task NumberPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(convoState);

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var numberPrompt = new NumberPrompt<int>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." } };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var numberResult = (int)results.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult}'.");
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
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(convoState);

            var dialogs = new DialogSet(dialogState);
            
            var numberPrompt = new NumberPrompt<int>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                        RetryPrompt = new Activity {  Type = ActivityTypes.Message, Text = "You must enter a number." }
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var numberResult = (int)results.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult}'.");
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
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(convoState);

            var dialogs = new DialogSet(dialogState);

            PromptValidator<int> validator = (ctx, promptContext, cancellationToken) =>
            {
                var result = promptContext.Recognized.Value;
                
                if (result < 100 && result > 0)
                {
                    promptContext.End(result);
                }
                return Task.CompletedTask;
            };
            var numberPrompt = new NumberPrompt<int>("NumberPrompt", validator, Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                        RetryPrompt = new Activity {  Type = ActivityTypes.Message, Text = "You must enter a positive number less than 100." }
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var numberResult = (int)results.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult}'.");
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
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(convoState);

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<float>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." }
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var numberResult = (float)results.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult}'.");
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
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(convoState);

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<long>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." }
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var numberResult = (long)results.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult}'.");
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
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(convoState);

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<double>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." }
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var numberResult = (double)results.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult}'.");
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
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(convoState);

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<decimal>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." }
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var numberResult = (decimal)results.Result;
                    await turnContext.SendActivityAsync($"Bot received the number '{numberResult}'.");
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
