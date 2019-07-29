// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class MiscTests
    {
        public TestContext TestContext { get; set; }

        private TestFlow CreateFlow(AdaptiveDialog dialog, bool sendTrace = false)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
            var resourceExplorer = new ResourceExplorer();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName), sendTrace);
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration(resourceExplorer)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            DialogManager dm = new DialogManager(dialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }

        [TestMethod]
        public async Task IfCondition_EndDialog()
        {
            var testDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Steps = new List<IDialog>()
                {
                    new TextInput()
                    {
                        Property = "user.name",
                        Prompt = new ActivityTemplate("name?")
                    },
                    new SendActivity("Hello, {user.name}")
                },
                Rules = new List<IRule>()
                {
                    new IntentRule("CancelIntent")
                    {
                        Steps = new List<IDialog>()
                        {
                            new ConfirmInput()
                            {
                                Property = "conversation.addTodo.cancelConfirmation",
                                Prompt = new ActivityTemplate("cancel?")
                            },
                            new IfCondition()
                            {
                                Condition = "conversation.addTodo.cancelConfirmation == true",
                                Steps = new List<IDialog>()
                                {
                                    new SendActivity("canceling"),
                                    new EndDialog()
                                },
                                ElseSteps = new List<IDialog>()
                                {
                                    new SendActivity("notcanceling")
                                }
                                // We do not need to specify an else block here since if user said no,
                                // the control flow will automatically return to the last active step (if any)
                            }
                        }
                    }
                },
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        { "HelpIntent", "(?i)help" },
                        { "CancelIntent", "(?i)cancel" }
                    }
                }
            };

            await CreateFlow(testDialog)
                .Send("hi")
                    .AssertReply("name?")
                .Send("cancel")
                    .AssertReply("cancel? (1) Yes or (2) No")
                .Send("yes")
                    .AssertReply("canceling")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Rule_Reprompt()
        {
            var testDialog = new AdaptiveDialog("testDialog")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        {  "SetName", @"my name is (?<name>.*)" }
                    }
                },
                Steps = new List<IDialog>()
                {
                    new TextInput() { Prompt = new ActivityTemplate("Hello, what is your name?"), OutputBinding = "user.name", AllowInterruptions = AllowInterruptions.Always , Value = "user.name"},
                    new SendActivity("Hello {user.name}, nice to meet you!"),
                    new NumberInput() { Prompt = new ActivityTemplate("What is your age?"), OutputBinding = "user.age" },
                    new SendActivity("{user.age} is a good age to be!"),
                    new SendActivity("your name is {user.name}!"),
                },
                Rules = new List<IRule>()
                {
                    new IntentRule("SetName", new List<string>() { "name" })
                    {
                        Steps = new List<IDialog>()
                        {
                            new SetProperty()
                            {
                                Property = "user.name",
                                Value = "@name"
                            }
                        }
                    }
                }

            };

            await CreateFlow(testDialog)
                .Send("hi")
                    .AssertReply("Hello, what is your name?")
                .Send("my name is Carlos")
                    .AssertReply("Hello Carlos, nice to meet you!")
                    .AssertReply("What is your age?")
                .Send("my name is Joe")
                    .AssertReply("What is your age?")
                .Send("15")
                    .AssertReply("15 is a good age to be!")
                    .AssertReply("your name is Joe!")
                .StartTestAsync();
        }
    }
}
