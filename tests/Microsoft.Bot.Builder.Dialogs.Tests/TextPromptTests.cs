// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class TextPromptTests
    {
        [TestMethod]
        public async Task TextPrompt()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            DialogSet dialogs = new DialogSet(dialogState);

            var textPrompt = new TextPrompt("TextPrompt");
            dialogs.Add(textPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter some text." } };
                    await dc.PromptAsync("TextPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var textResult = (string)results.Result;
                    await turnContext.SendActivityAsync($"Bot received the text '{textResult}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter some text.")
            .Send("some text")
            .AssertReply("Bot received the text 'some text'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TextPromptValidator()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            DialogSet dialogs = new DialogSet(dialogState);

            PromptValidator<string> validator = async (ctx, promptContext) =>
            {
                var value = promptContext.Recognized.Value;
                if (value.Length <= 3)
                {
                    await ctx.SendActivityAsync("Make sure the text is greater than three characters.");
                }
                else
                {
                    promptContext.End(value);
                }
            };
            var textPrompt = new TextPrompt("TextPrompt", validator);
            dialogs.Add(textPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter some text." } };
                    await dc.PromptAsync("TextPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var textResult = (string)results.Result;
                    await turnContext.SendActivityAsync($"Bot received the text '{textResult}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter some text.")
            .Send("hi")
            .AssertReply("Make sure the text is greater than three characters.")
            .Send("hello")
            .AssertReply("Bot received the text 'hello'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TextPromptWithRetryPrompt()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            DialogSet dialogs = new DialogSet(dialogState);

            PromptValidator<string> validator = async (ctx, promptContext) =>
            {
                var value = promptContext.Recognized.Value;
                if (value.Length >= 3)
                {
                    promptContext.End(value);
                }
            };
            var textPrompt = new TextPrompt("TextPrompt", validator);
            dialogs.Add(textPrompt);
        
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter some text." },
                        RetryPrompt = new Activity { Type = ActivityTypes.Message, Text = "Make sure the text is greater than three characters." },
                    };
                    await dc.PromptAsync("TextPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var textResult = (string)results.Result;
                    await turnContext.SendActivityAsync($"Bot received the text '{textResult}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter some text.")
            .Send("hi")
            .AssertReply("Make sure the text is greater than three characters.")
            .Send("hello")
            .AssertReply("Bot received the text 'hello'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TextPromptValidatorWithMessageShouldNotSendRetryPrompt()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            DialogSet dialogs = new DialogSet(dialogState);

            PromptValidator<string> validator = async (ctx, promptContext) =>
            {
                var value = promptContext.Recognized.Value;
                if (value.Length <= 3)
                {
                    await ctx.SendActivityAsync("The text should be greater than 3 chars.");
                }
                else
                {
                    promptContext.End(value);
                }
            };
            var textPrompt = new TextPrompt("TextPrompt", validator);
            dialogs.Add(textPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter some text." },
                        RetryPrompt = new Activity { Type = ActivityTypes.Message, Text = "Make sure the text is greater than three characters." },
                    };
                    await dc.PromptAsync("TextPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    var textResult = (string)results.Result;
                    await turnContext.SendActivityAsync($"Bot received the text '{textResult}'.");
                }
            })
            .Send("hello")
            .AssertReply("Enter some text.")
            .Send("hi")
            .AssertReply("The text should be greater than 3 chars.")
            .Send("hello")
            .AssertReply("Bot received the text 'hello'.")
            .StartTestAsync();
        }
    }
}
