// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs.Declarative.Expressions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Rules.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Rules.Rules;
using Microsoft.Bot.Builder.Dialogs.Rules.Steps;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Tests
{
    [TestClass]
    public class StepsTests
    {
        public TestContext TestContext { get; set; }

        private TestFlow CreateFlow(RuleDialog planningDialog, ConversationState convoState, UserState userState)
        {
            var botResourceManager = new BotResourceManager();
            var lg = new LGLanguageGenerator(botResourceManager);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new RegisterClassMiddleware<IBotResourceProvider>(botResourceManager))
                .Use(new RegisterClassMiddleware<ILanguageGenerator>(lg))
                .Use(new RegisterClassMiddleware<IMessageActivityGenerator>(new TextMessageActivityGenerator(lg)))
                .Use(new AutoSaveStateMiddleware(convoState, userState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var convoStateProperty = convoState.CreateProperty<Dictionary<string, object>>("conversation");

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            planningDialog.BotState = convoState.CreateProperty<BotState>("bot");
            planningDialog.UserState = userState.CreateProperty<StateMap>("user"); ;
            planningDialog.Storage = new MemoryStorage();

            var dialogs = new DialogSet(dialogState);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await planningDialog.OnTurnAsync(turnContext, null).ConfigureAwait(false);
            });
        }


        [TestMethod]
        public async Task Planning_WaitForInput()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new RuleDialog("planningTest");

            planningDialog.AddRule(new List<IRule>()
            {
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new SendActivity("Hello, what is your name?"),
                        new WaitForInput("user.name"),
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
        public async Task Planning_IfProperty()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new RuleDialog("planningTest");

            planningDialog.AddRule(new List<IRule>()
            {
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new IfProperty()
                        {
                            Expression = new CommonExpression("user.name == null"),
                            IfTrue = new List<IDialog>()
                            {
                                new SendActivity("Hello, what is your name?"),
                                new WaitForInput("user.name"),
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
        public async Task Planning_TextPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new RuleDialog("planningTest");

            planningDialog.AddRule(new List<IRule>()
            {
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new IfProperty()
                        {
                            Expression = new CommonExpression("user.name == null"),
                            IfTrue = new List<IDialog>()
                            {
                                new TextPrompt()
                                {
                                    InitialPrompt = new ActivityTemplate("Hello, what is your name?"),
                                    Property = "user.name"
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
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Planning_DoSteps()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new RuleDialog("planningTest");

            planningDialog.Recognizer = new RegexRecognizer() { Rules = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            planningDialog.AddRule(new List<IRule>()
            {
                new IntentRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new WaitForInput(),
                        new SendActivity("To get to the other side")
                    }),
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new IfProperty()
                        {
                            Expression = new CommonExpression("user.name == null"),
                            IfTrue = new List<IDialog>()
                            {
                                new TextPrompt()
                                {
                                    InitialPrompt = new ActivityTemplate("Hello, what is your name?"),
                                    OutputBinding = "user.name"
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
        public async Task Planning_CallDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new RuleDialog("planningTest");

            planningDialog.Recognizer = new RegexRecognizer() { Rules = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            var tellJokeDialog = new RuleDialog("TellJokeDialog");
            tellJokeDialog.AddRule(new List<IRule>()
            {
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new WaitForInput(),
                        new SendActivity("To get to the other side")
                    }
                 )
            });

            var askNameDialog = new RuleDialog("AskNameDialog");
            askNameDialog.AddRule(new List<IRule>()
            {
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new IfProperty()
                        {
                            Expression = new CommonExpression("user.name == null"),
                            IfTrue = new List<IDialog>()
                            {
                                new TextPrompt()
                                {
                                    InitialPrompt = new ActivityTemplate("Hello, what is your name?"),
                                    OutputBinding = "user.name"
                                }
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    })
            });

            planningDialog.AddRule(new List<IRule>()
            {
                new ReplacePlanRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new CallDialog() { Dialog = tellJokeDialog }
                    }),
                new WelcomeRule(
                    steps: new List<IDialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                    }),
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new CallDialog() { Dialog = askNameDialog }
                    })
            });

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
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
        public async Task Planning_GotoDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new RuleDialog("planningTest");

            planningDialog.Recognizer = new RegexRecognizer() { Rules = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            var tellJokeDialog = new RuleDialog("TellJokeDialog");
            tellJokeDialog.AddRule(new List<IRule>()
            {
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new WaitForInput(),
                        new SendActivity("To get to the other side")
                    }
                 )
            });

            var askNameDialog = new RuleDialog("AskNameDialog");
            askNameDialog.AddRule(new List<IRule>()
            {
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new IfProperty()
                        {
                            Expression = new CommonExpression("user.name == null"),
                            IfTrue = new List<IDialog>()
                            {
                                new TextPrompt()
                                {
                                    InitialPrompt = new ActivityTemplate("Hello, what is your name?"),
                                    OutputBinding = "user.name"
                                }
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    })
            });

            planningDialog.AddRule(new List<IRule>()
            {
                new ReplacePlanRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new GotoDialog() { DialogId = "TellJokeDialog" }
                    }),
                new WelcomeRule(
                    steps: new List<IDialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                    }),
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new GotoDialog() { DialogId = "AskNameDialog" }
                    })
            });

            planningDialog.AddDialog(new List<IDialog>()
            {
                tellJokeDialog,
                askNameDialog
            });

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
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
        public async Task Planning_EndDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new RuleDialog("planningTest");

            planningDialog.Recognizer = new RegexRecognizer() { Rules = new Dictionary<string, string>() { { "EndIntent", "end" } } };

            var tellJokeDialog = new RuleDialog("TellJokeDialog");
            tellJokeDialog.AddRule(new List<IRule>()
            {
                new IntentRule("EndIntent",
                    steps: new List<IDialog>()
                    {
                        new EndDialog()
                    }),
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new WaitForInput(),
                        new SendActivity("To get to the other side")
                    }
                 )
            });
            tellJokeDialog.Recognizer = new RegexRecognizer() { Rules = new Dictionary<string, string>() { { "EndIntent", "end" } } };

            planningDialog.AddRule(new List<IRule>()
            {
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new CallDialog() { Dialog = tellJokeDialog },
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
