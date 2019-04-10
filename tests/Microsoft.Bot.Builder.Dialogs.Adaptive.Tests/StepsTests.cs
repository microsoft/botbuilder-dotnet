// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class StepsTests
    {
        public TestContext TestContext { get; set; }

        private TestFlow CreateFlow(AdaptiveDialog planningDialog, ConversationState convoState, UserState userState)
        {
            var botResourceManager = new ResourceExplorer();
            var lg = new LGLanguageGenerator(botResourceManager);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new RegisterClassMiddleware<ResourceExplorer>(botResourceManager))
                .Use(new RegisterClassMiddleware<ILanguageGenerator>(lg))
                .Use(new RegisterClassMiddleware<IStorage>(new MemoryStorage()))
                .Use(new RegisterClassMiddleware<IMessageActivityGenerator>(new TextMessageActivityGenerator(lg)))
                .Use(new AutoSaveStateMiddleware(convoState, userState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var userStateProperty = userState.CreateProperty<Dictionary<string, object>>("user");
            var convoStateProperty = convoState.CreateProperty<Dictionary<string, object>>("conversation");

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);


            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await planningDialog.OnTurnAsync(turnContext, null).ConfigureAwait(false);
            });
        }


        [TestMethod]
        public async Task Step_WaitForInput()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest");

            planningDialog.AddRules(new List<IRule>()
            {
                new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new TextInput() { Prompt = new ActivityTemplate("Hello, what is your name?"),  OutputProperty = "user.name" },
                        new SendActivity("Hello {user.name}, nice to meet you!"),
                    })});

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Step_IfCondition()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest");

            planningDialog.AddRules(new List<IRule>()
            {
                new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new IfCondition()
                        {
                            Condition = new ExpressionEngine().Parse("user.name == null"),
                            Steps = new List<IDialog>()
                            {
                                new TextInput() {
                                    Prompt  = new ActivityTemplate("Hello, what is your name?"),
                                    OutputProperty = "user.name"
                                },
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    })});

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Step_Switch()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest")
            {
                Steps = new List<IDialog>()
                    {
                        new SetProperty()
                        {
                            Property = "user.name",
                            Value = new ExpressionEngine().Parse("'frank'")
                        },
                        new SwitchCondition()
                        {
                            Condition = new ExpressionEngine().Parse("user.name"),
                            Cases = new Dictionary<string, List<IDialog>>()
                            {
                                { "susan", new List<IDialog>() { new SendActivity("hi susan") } },
                                { "bob", new List<IDialog>() { new SendActivity("hi bob") } },
                                { "frank", new List<IDialog>() { new SendActivity("hi frank") } }
                            },
                            Default = new List<IDialog>() { new SendActivity("Who are you?") }
                        },
                    }
            };

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("hi frank")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Step_TextInput()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest");

            planningDialog.AddRules(new List<IRule>()
            {
                new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new IfCondition()
                        {
                            Condition = new ExpressionEngine().Parse("user.name == null"),
                            Steps = new List<IDialog>()
                            {
                                new TextInput()
                                {
                                    Prompt = new ActivityTemplate("Hello, what is your name?"),
                                    RetryPrompt = new ActivityTemplate("How should I call you?"),
                                    Property = "user.name",
                                    Pattern = @"(\s*(\S)\s*){3,}"
                                }
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    })});

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("c")
                .AssertReply("How should I call you?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Step_TextInputWithInvalidPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest");

            planningDialog.AddRules(new List<IRule>()
            {
                new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new IfCondition()
                        {
                            Condition = new ExpressionEngine().Parse("user.name == null"),
                            Steps = new List<IDialog>()
                            {
                                new TextInput()
                                {
                                    Prompt = new ActivityTemplate("Hello, what is your name?"),
                                    RetryPrompt = new ActivityTemplate("How should I call you?"),
                                    InvalidPrompt  = new ActivityTemplate("That does not soud like a name"),
                                    Property = "user.name",
                                    Pattern = @"(\s*(\S)\s*){3,}"
                                }
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    })});

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("c")
                .AssertReply("That does not soud like a name")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Step_DoSteps()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest");

            planningDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            planningDialog.AddRules(new List<IRule>()
            {
                new IntentRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new EndTurn(),
                        new SendActivity("To get to the other side")
                    }),
                new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new IfCondition()
                        {
                            Condition = new ExpressionEngine().Parse("user.name == null"),
                            Steps = new List<IDialog>()
                            {
                                new TextInput()
                                {
                                    Prompt  = new ActivityTemplate("Hello, what is your name?"),
                                    OutputProperty = "user.name"
                                }
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    })});

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .Send("Do you know a joke?")
                .AssertReply("Why did the chicken cross the road?")
            .Send("Why?")
                .AssertReply("To get to the other side")
            .Send("hi")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }


        [TestMethod]
        public async Task Step_BeginDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var tellJokeDialog = new AdaptiveDialog("TellJokeDialog");
            tellJokeDialog.AddRules(new List<IRule>()
            {
                new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new EndTurn(),
                        new SendActivity("To get to the other side")
                    }
                 )
            });

            var askNameDialog = new AdaptiveDialog("AskNameDialog")
            {
                Steps = new List<IDialog>()
                {
                    new IfCondition()
                    {
                        Condition = new ExpressionEngine().Parse("user.name == null"),
                        Steps = new List<IDialog>()
                        {
                            new TextInput()
                            {
                                Prompt  = new ActivityTemplate("Hello, what is your name?"),
                                OutputProperty = "user.name"
                            }
                        }
                    },
                    new SendActivity("Hello {user.name}, nice to meet you!")
                }
            };


            var planningDialog = new AdaptiveDialog("planningTest");
            planningDialog.AutoEndDialog = false;

            planningDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            planningDialog.Steps = new List<IDialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'"),
                        new BeginDialog() { Dialog = askNameDialog }
                    };

            planningDialog.AddRules(new List<IRule>()
            {
                new IntentRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new BeginDialog() { Dialog = tellJokeDialog }
                    }),
                new UnknownIntentRule(
                    steps: new List<IDialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                    }),
            });

            await CreateFlow(planningDialog, convoState, userState)
            .SendConversationUpdate()
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .Send("Cool")
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Step_ReplaceDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var tellJokeDialog = new AdaptiveDialog("TellJokeDialog");
            tellJokeDialog.AddRules(new List<IRule>()
            {
                new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new EndTurn(),
                        new SendActivity("To get to the other side")
                    }
                 )
            });

            var askNameDialog = new AdaptiveDialog("AskNameDialog")
            {
                Steps = new List<IDialog>()
                {
                    new IfCondition()
                    {
                        Condition = new ExpressionEngine().Parse("user.name == null"),
                        Steps = new List<IDialog>()
                        {
                            new TextInput()
                            {
                                Prompt = new ActivityTemplate("Hello, what is your name?"),
                                RetryPrompt = new ActivityTemplate("How should I call you?"),
                                InvalidPrompt  = new ActivityTemplate("That does not soud like a name"),
                                Property = "user.name",
                                Pattern = @"(\s*(\S)\s*){3,}"
                            }
                        }
                    },
                    new SendActivity("Hello {user.name}, nice to meet you!")
                }
            };

            var planningDialog = new AdaptiveDialog("planningTest");
            planningDialog.AutoEndDialog = false;
            planningDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };
            planningDialog.Steps = new List<IDialog>()
            {
                new SendActivity("I'm a joke bot. To get started say 'tell me a joke'"),
                new ReplaceDialog("AskNameDialog")
            };

            planningDialog.AddRules(new List<IRule>()
            {
                new IntentRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new ReplaceDialog("TellJokeDialog")
                    }),
            });

            planningDialog.AddDialog(new List<IDialog>()
            {
                tellJokeDialog,
                askNameDialog
            });

            await CreateFlow(planningDialog, convoState, userState)
            .SendConversationUpdate()
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .Send("Do you know a joke?")
                .AssertReply("Why did the chicken cross the road?")
            .Send("Why?")
                .AssertReply("To get to the other side")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Step_EndDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest");

            planningDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "EndIntent", "end" } } };

            var tellJokeDialog = new AdaptiveDialog("TellJokeDialog");
            tellJokeDialog.AddRules(new List<IRule>()
            {
                new IntentRule("EndIntent",
                    steps: new List<IDialog>()
                    {
                        new EndDialog()
                    }),
                new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new EndTurn(),
                        new SendActivity("To get to the other side")
                    }
                 )
            });
            tellJokeDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "EndIntent", "end" } } };

            planningDialog.AddRules(new List<IRule>()
            {
                new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new BeginDialog() { Dialog = tellJokeDialog },
                        new SendActivity("You went out from ask name dialog.")
                    })
            });

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Why did the chicken cross the road?")
            .Send("end")
                .AssertReply("You went out from ask name dialog.")
            .StartTestAsync();
        }


    }
}
