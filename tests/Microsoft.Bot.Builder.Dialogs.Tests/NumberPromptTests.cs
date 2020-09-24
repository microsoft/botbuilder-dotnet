// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class NumberPromptTests
    {
        [Fact]
        public void NumberPromptWithEmptyIdShouldFail()
        {
            Assert.Throws<ArgumentNullException>(() => new NumberPrompt<int>(string.Empty));
        }

        [Fact]
        public void NumberPromptWithNullIdShouldFail()
        {
            Assert.Throws<ArgumentNullException>(() => new NumberPrompt<int>(null));
        }

        [Fact]
        public void NumberPromptWithUnsupportedTypeShouldFail()
        {
            Assert.Throws<NotSupportedException>(() => new NumberPrompt<short>("prompt"));
        }

        [Fact]
        public async Task NumberPromptWithNullTurnContextShouldFail()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                var numberPromptMock = new NumberPromptMock("NumberPromptMock");

                var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Please send a number." } };

                await numberPromptMock.OnPromptNullContext(options);
            });
        }

        [Fact]
        public async Task OnPromptErrorsWithNullOptions()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                var convoState = new ConversationState(new MemoryStorage());
                var dialogState = convoState.CreateProperty<DialogState>("dialogState");

                var adapter = new TestAdapter()
                    .Use(new AutoSaveStateMiddleware(convoState));

                // Create new DialogSet.
                var dialogs = new DialogSet(dialogState);

                // Create and add custom activity prompt to DialogSet.
                var numberPromptMock = new NumberPromptMock("NumberPromptMock");
                dialogs.Add(numberPromptMock);

                await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                    await numberPromptMock.OnPromptNullOptions(dc);
                })
                .Send("hello")
                .StartTestAsync();
            });
        }

        [Fact]
        public async Task OnRecognizeWithNullTurnContextShouldFail()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                var numberPromptMock = new NumberPromptMock("NumberPromptMock");

                await numberPromptMock.OnRecognizeNullContext();
            });
        }

        [Fact]
        public async Task NumberPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var numberPrompt = new NumberPrompt<int>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." } };
                    await dc.PromptAsync("NumberPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (int)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{numberResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("42")
            .AssertReply("Bot received the number '42'.")
            .StartTestAsync();
        }

        [Fact]
        public async Task NumberPromptRetry()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<int>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                        RetryPrompt = new Activity { Type = ActivityTypes.Message, Text = "You must enter a number." },
                    };
                    await dc.PromptAsync("NumberPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (int)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{numberResult}'."), cancellationToken);
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

        [Fact]
        public async Task NumberPromptValidator()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            PromptValidator<int> validator = (promptContext, cancellationToken) =>
            {
                var result = promptContext.Recognized.Value;

                if (result < 100 && result > 0)
                {
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            };
            var numberPrompt = new NumberPrompt<int>("NumberPrompt", validator, Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                        RetryPrompt = new Activity { Type = ActivityTypes.Message, Text = "You must enter a positive number less than 100." },
                    };
                    await dc.PromptAsync("NumberPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (int)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{numberResult}'."), cancellationToken);
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

        [Fact]
        public async Task FloatNumberPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<float>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                    };
                    await dc.PromptAsync("NumberPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (float)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{numberResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("3.14")
            .AssertReply("Bot received the number '3.14'.")
            .StartTestAsync();
        }

        [Fact]
        public async Task LongNumberPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<long>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                    };
                    await dc.PromptAsync("NumberPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (long)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{numberResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("42")
            .AssertReply("Bot received the number '42'.")
            .StartTestAsync();
        }

        [Fact]
        public async Task DoubleNumberPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<double>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (double)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{numberResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("3.14")
            .AssertReply("Bot received the number '3.14'.")
            .StartTestAsync();
        }

        [Fact]
        public async Task CurrencyNumberPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<double>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (double)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{numberResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("$500")
            .AssertReply("Bot received the number '500'.")
            .StartTestAsync();
        }

        [Fact]
        public async Task AgeNumberPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<double>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (double)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{numberResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("i am 18 years old")
            .AssertReply("Bot received the number '18'.")
            .StartTestAsync();
        }

        [Fact]
        public async Task DimensionNumberPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<double>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (double)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{numberResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("I've run 5km")
            .AssertReply("Bot received the number '5'.")
            .StartTestAsync();
        }

        [Fact]
        public async Task TemperatureNumberPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<double>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (double)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{numberResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("The temperature is 32C")
            .AssertReply("Bot received the number '32'.")
            .StartTestAsync();
        }

        [Fact]
        public async Task CultureThruNumberPromptCtor()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<double>("NumberPrompt", defaultLocale: Culture.Dutch);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (double)results.Result;
                    Assert.Equal(3.14, numberResult);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("3,14")
            .StartTestAsync();
        }

        [Fact]
        public async Task CultureThruActivityNumberPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<double>("NumberPrompt", defaultLocale: Culture.Dutch);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (double)results.Result;
                    Assert.Equal(3.14, numberResult);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send(new Activity { Type = ActivityTypes.Message, Text = "3,14", Locale = Culture.Dutch })
            .StartTestAsync();
        }

        [Fact]
        public async Task NumberPromptDefaultsToEnUsLocale()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<double>("NumberPrompt");
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                    };
                    await dc.PromptAsync("NumberPrompt", options);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (double)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{numberResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("3.14")
            .AssertReply("Bot received the number '3.14'.")
            .StartTestAsync();
        }

        [Fact]
        public async Task DecimalNumberPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<decimal>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                    };
                    await dc.PromptAsync("NumberPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (decimal)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{numberResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("3.14")
            .AssertReply("Bot received the number '3.14'.")
            .StartTestAsync();
        }

        [Fact]
        public async Task NoNumberButUnitPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var numberPrompt = new NumberPrompt<decimal>("NumberPrompt", defaultLocale: Culture.English);
            dialogs.Add(numberPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter a number." },
                    };
                    await dc.PromptAsync("NumberPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var numberResult = (decimal)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{numberResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("dollars")
            .AssertReply("Enter a number.")
            .Send("500 dollars")
            .AssertReply("Bot received the number '500'.")
            .StartTestAsync();
        }
    }
}
