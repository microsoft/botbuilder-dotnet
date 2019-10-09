// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.LanguageGeneration.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class MiscTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task IfCondition_EndDialog()
        {
            var testDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new TextInput()
                            {
                                Property = "user.name",
                                Prompt = new ActivityTemplate("name?")
                            },
                            new SendActivity("Hello, {user.name}")
                        },
                    },
                    new OnIntent("CancelIntent")
                    {
                        Actions = new List<Dialog>()
                        {
                            new ConfirmInput()
                            {
                                Property = "conversation.addTodo.cancelConfirmation",
                                Prompt = new ActivityTemplate("cancel?")
                            },
                            new IfCondition()
                            {
                                Condition = "conversation.addTodo.cancelConfirmation == true",
                                Actions = new List<Dialog>()
                                {
                                    new SendActivity("canceling"),
                                    new EndDialog()
                                },
                                ElseActions = new List<Dialog>()
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
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("HelpIntent", "(?i)help"),
                        new IntentPattern("CancelIntent", "(?i)cancel"),
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
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("SetName", @"my name is (?<name>.*)"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new TextInput() { Prompt = new ActivityTemplate("Hello, what is your name?"), Property = "user.name", AllowInterruptions = AllowInterruptions.Always },
                            new SendActivity("Hello {user.name}, nice to meet you!"),
                            new NumberInput() { Prompt = new ActivityTemplate("What is your age?"), Property = "user.age" },
                            new SendActivity("{user.age} is a good age to be!"),
                            new SendActivity("your name is {user.name}!"),
                        },
                    },
                    new OnIntent("SetName", new List<string>() { "name" })
                    {
                        Actions = new List<Dialog>()
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
                .UseAdaptiveDialogs()
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            DialogManager dm = new DialogManager(dialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }
    }
}
