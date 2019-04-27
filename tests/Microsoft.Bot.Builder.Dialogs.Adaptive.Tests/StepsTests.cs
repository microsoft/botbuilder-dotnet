// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.LanguageGeneration.Renderer;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class StepsTests
    {
        public TestContext TestContext { get; set; }

        private TestFlow CreateFlow(AdaptiveDialog planningDialog, ConversationState convoState, UserState userState, bool sendTrace = false)
        {
            var botResourceManager = new ResourceExplorer();
            var lg = new LGLanguageGenerator(botResourceManager);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName), sendTrace)
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
                        new TextInput() { Prompt = new ActivityTemplate("Hello, what is your name?"),  OutputBinding = "user.name" },
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
        public async Task Step_TraceActivity()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var dialog = new AdaptiveDialog("traceActivity");

            dialog.AddRules(new List<IRule>()
            {
                new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new SetProperty()
                        {
                             Property = "user.name",
                             Value = new ExpressionEngine().Parse("'frank'")
                        },
                        new TraceActivity()
                        {
                            Name = "test",
                            ValueType = "user",
                            Value = "user"
                        },
                        new TraceActivity()
                        {
                            Name = "test",
                            ValueType = "memory"
                        }
                    })});

            await CreateFlow(dialog, convoState, userState, sendTrace: true)
            .Send("hi")
                .AssertReply((activity) =>
                {
                    var traceActivity = (ITraceActivity)activity;
                    Assert.AreEqual(ActivityTypes.Trace, traceActivity.Type, "type doesn't match");
                    Assert.AreEqual("user", traceActivity.ValueType, "ValueType doesn't match");
                    Assert.AreEqual("frank", (string)((dynamic)traceActivity.Value).name, "Value doesn't match");
                })
                .AssertReply((activity) =>
                {
                    var traceActivity = (ITraceActivity)activity;
                    Assert.AreEqual(ActivityTypes.Trace, traceActivity.Type, "type doesn't match");
                    Assert.AreEqual("memory", traceActivity.ValueType, "ValueType doesn't match");
                    Assert.AreEqual("frank", (string)((dynamic)traceActivity.Value).user.name, "Value doesn't match");
                })
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
                                    OutputBinding = "user.name"
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
                            Condition = "user.name",
                            Cases = new List<Case>()
                            {
                                new Case("'susan'", new List<IDialog>() { new SendActivity("hi susan") } ),
                                new Case("'bob'", new List<IDialog>() { new SendActivity("hi bob") } ),
                                new Case("'frank'", new List<IDialog>() { new SendActivity("hi frank") } )
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
        public async Task Step_ConfirmInput()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Steps = new List<IDialog>()
                {
                    new ConfirmInput()
                    {
                        Prompt = new ActivityTemplate("yes or no"),
                        RetryPrompt = new ActivityTemplate("I need a yes or no."),
                        Property = "user.confirmed"
                    },
                    new SendActivity("confirmation: {user.confirmed}"),
                    new ConfirmInput()
                    {
                        Prompt = new ActivityTemplate("yes or no"),
                        RetryPrompt = new ActivityTemplate("I need a yes or no."),
                        Property = "user.confirmed"
                    },
                    new SendActivity("confirmation: {user.confirmed}"),
                    new ConfirmInput()
                    {
                        Prompt = new ActivityTemplate("yes or no"),
                        RetryPrompt = new ActivityTemplate("I need a yes or no."),
                        Property = "user.confirmed",
                        AlwaysPrompt = true
                    },
                    new SendActivity("confirmation: {user.confirmed}"),
                }
            };

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("yes or no")
            .Send("asdasd")
                .AssertReply("I need a yes or no.")
            .Send("yes")
                .AssertReply("confirmation: True")
                .AssertReply("confirmation: True")
                .AssertReply("yes or no")
            .Send("nope")
                .AssertReply("confirmation: False")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Step_ChoiceInput()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Steps = new List<IDialog>()
                {
                    new ChoiceInput()
                    {
                        Property = "user.color",
                        Prompt = new ActivityTemplate("Please select a color:"),
                        RetryPrompt = new ActivityTemplate("Not a color. Please select a color:"),
                        Choices = new List<Choice>() { new Choice("red"), new Choice("green"), new Choice("blue") },
                        Style = ListStyle.Inline
                    },
                    new SendActivity("{user.color}"),
                    new ChoiceInput()
                    {
                        Property = "user.color",
                        Prompt = new ActivityTemplate("Please select a color:"),
                        RetryPrompt = new ActivityTemplate("Please select a color:"),
                        Choices = new List<Choice>() { new Choice("red"), new Choice("green"), new Choice("blue") },
                        Style = ListStyle.Inline
                    },
                    new SendActivity("{user.color}"),
                    new ChoiceInput()
                    {
                        Property = "user.color",
                        Prompt = new ActivityTemplate("Please select a color:"),
                        RetryPrompt = new ActivityTemplate("Please select a color:"),
                        Choices = new List<Choice>() { new Choice("red"), new Choice("green"), new Choice("blue") },
                        AlwaysPrompt = true,
                        Style = ListStyle.Inline
                    },
                    new SendActivity("{user.color}"),
                }
            };

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Please select a color: (1) red, (2) green, or (3) blue")
            .Send("asdasd")
                .AssertReply("Not a color. Please select a color: (1) red, (2) green, or (3) blue")
            .Send("blue")
                .AssertReply("blue")
                .AssertReply("blue")
                .AssertReply("Please select a color: (1) red, (2) green, or (3) blue")
            .Send("red")
                .AssertReply("red")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Step_NumberInput()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false
            };

            planningDialog.AddRules(new List<IRule>()
            {
                new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new NumberInput()
                        {
                            Prompt = new ActivityTemplate("Please enter your age."),
                            MinValue = 1,
                            MaxValue = 150,
                            Precision = 0,
                            RetryPrompt = new ActivityTemplate("The value entered must be greater than 0 and less than 150."),
                            Property = "user.userProfile.Age"
                        },
                        new SendActivity("I have your age as {user.userProfile.Age}."),
                    })
            });

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Please enter your age.")
            .Send("1000")
                .AssertReply("The value entered must be greater than 0 and less than 150.")
            .Send("15.3")
                .AssertReply("I have your age as 15.")
            .Send("hi")
                .AssertReply("I have your age as 15.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Step_NumberInputPrecision()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false
            };

            planningDialog.AddRules(new List<IRule>()
            {
                new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new NumberInput()
                        {
                            Prompt = new ActivityTemplate("Please enter your dollars."),
                            MinValue = 10.00f,
                            MaxValue = 100.00f,
                            Precision = 2,
                            RetryPrompt = new ActivityTemplate("The value entered must be greater than 10.00 and less than 100.00."),
                            Property = "user.userProfile.dollars"
                        },
                        new SendActivity("I have your dollars as {user.userProfile.dollars}."),
                    })
            });

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Please enter your dollars.")
            .Send("1.345")
                .AssertReply("The value entered must be greater than 10.00 and less than 100.00.")
            .Send("15.348")
                .AssertReply("I have your dollars as 15.35.")
            .Send("hi")
                .AssertReply("I have your dollars as 15.35.")
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
                                OutputBinding = "user.name"
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

        [TestMethod]
        public async Task Step_RepeatDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var testDialog = new AdaptiveDialog("testDialog")
            {
                Steps = new List<IDialog>()
                {
                    new TextInput() { Prompt = new ActivityTemplate("Hello, what is your name?"), OutputBinding = "user.name" },
                    new SendActivity("Hello {user.name}, nice to meet you!"),
                    new EndTurn(),
                    new RepeatDialog()
                }
            };

            await CreateFlow(testDialog, convoState, userState)
                .Send("hi")
                    .AssertReply("Hello, what is your name?")
                .Send("Carlos")
                    .AssertReply("Hello Carlos, nice to meet you!")
                .Send("hi")
                    .AssertReply("Hello Carlos, nice to meet you!")
                .StartTestAsync();
        }


        [TestMethod]
        public async Task Step_EmitEvent()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var rootDialog = new AdaptiveDialog("root")
            {
                Steps = new List<IDialog>()
                {
                    new BeginDialog()
                    {
                        Dialog = new AdaptiveDialog("outer")
                        {
                            AutoEndDialog = false,
                            Recognizer = new RegexRecognizer()
                            {
                                Intents = new Dictionary<string, string>()
                                {
                                    { "EmitIntent" , "emit" },
                                    { "CowboyIntent" , "moo" }
                                }
                            },
                            Rules = new List<IRule>()
                            {
                                new IntentRule(intent: "CowboyIntent")
                                {
                                    Steps = new List<IDialog>()
                                    {
                                        new SendActivity("Yippee ki-yay!")
                                    }
                                },
                                new IntentRule(intent: "EmitIntent")
                                {
                                    Steps = new List<IDialog>()
                                    {
                                        new EmitEvent()
                                        {
                                            EventName = "CustomEvent",
                                            BubbleEvent = true,
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Rules = new List<IRule>()
                {
                    new EventRule()
                    {
                        Events = new List<string>() { "CustomEvent"},
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("CustomEventFired")
                        }
                    }
                }
            };


            await CreateFlow(rootDialog, convoState, userState)
            .Send("moo")
                .AssertReply("Yippee ki-yay!")
            .Send("emit")
                .AssertReply("CustomEventFired")
            .Send("moo")
                .AssertReply("Yippee ki-yay!")
            .StartTestAsync();
        }
    }
}
