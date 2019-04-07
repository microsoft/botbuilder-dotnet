// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class AdaptiveDialogTests
    {
        public TestContext TestContext { get; set; }

        private TestFlow CreateFlow(AdaptiveDialog ruleDialog, ConversationState convoState, UserState userState)
        {
            var explorer = new ResourceExplorer();
            var lg = new LGLanguageGenerator(explorer);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new RegisterClassMiddleware<IStorage>(new MemoryStorage()))
                .Use(new RegisterClassMiddleware<IExpressionParser>(new ExpressionEngine()))
                .Use(new RegisterClassMiddleware<ResourceExplorer>(explorer))
                .Use(new RegisterClassMiddleware<ILanguageGenerator>(lg))
                .Use(new RegisterClassMiddleware<IMessageActivityGenerator>(new TextMessageActivityGenerator(lg)))
                .Use(new AutoSaveStateMiddleware(convoState, userState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var convoStateProperty = convoState.CreateProperty<Dictionary<string, object>>("conversation");

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            ruleDialog.BotState = convoState.CreateProperty<BotState>("bot");
            ruleDialog.UserState = userState.CreateProperty<Dictionary<string, object>>("user"); ;

            var dialogs = new DialogSet(dialogState);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await ruleDialog.OnTurnAsync(turnContext, null).ConfigureAwait(false);
            });
        }

        [TestMethod]
        public async Task AdaptiveDialog_TopLevelFallback()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(new NoneIntentRule(
                    new List<IDialog>()
                    {
                        new SendActivity("Hello Planning!")
                    }));

            await CreateFlow(ruleDialog, convoState, userState)
            .Send("start")
                .AssertReply("Hello Planning!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_TopLevelFallbackMultipleActivities()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(new NoneIntentRule(new List<IDialog>()
                    {
                        new SendActivity("Hello Planning!"),
                        new SendActivity("Howdy awain")
                    }));

            await CreateFlow(ruleDialog, convoState, userState)
            .Send("start")
                .AssertReply("Hello Planning!")
                .AssertReply("Howdy awain")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_EndTurn()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(
                new NoneIntentRule(
                    new List<IDialog>()
                    {
                        new TextInput()
                        {
                            Prompt = new ActivityTemplate("Hello, what is your name?"),
                            Property = "user.name"
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!"),
                    }));

            await CreateFlow(ruleDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_EditArray()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var dialog = new AdaptiveDialog("planningTest");
            dialog.Steps = new List<IDialog>()
            {
                // Add item
                new TextInput() {
                    AlwaysPrompt = true,
                    Prompt = new ActivityTemplate("Please add an item to todos."),
                    Property = "dialog.todo"
                },
                new EditArray(EditArray.ArrayChangeType.Push, "user.todos", "dialog.todo"),
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ',')}") },
                new TextInput()
                {
                    AlwaysPrompt = true,
                    Prompt = new ActivityTemplate("Please add an item to todos."),
                    Property = "dialog.todo"

                },
                new EditArray(EditArray.ArrayChangeType.Push, "user.todos", "dialog.todo"),
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ',')}") },

                // Remove item
                new TextInput() {
                    AlwaysPrompt = true,
                    Prompt = new ActivityTemplate("Enter a item to remove."),
                    Property = "dialog.todo"
                },
                new EditArray(EditArray.ArrayChangeType.Remove, "user.todos", "dialog.todo"),
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ',')}") },

                // Add item and pop item
                new TextInput() {
                    AlwaysPrompt = true,
                    Prompt = new ActivityTemplate("Please add an item to todos."),
                    Property = "dialog.todo"
                },
                new EditArray(EditArray.ArrayChangeType.Push, "user.todos", "dialog.todo"),
                new TextInput()
                {
                    AlwaysPrompt = true,
                    Prompt = new ActivityTemplate("Please add an item to todos."),
                    Property = "dialog.todo"
                },
                new EditArray(EditArray.ArrayChangeType.Push, "user.todos", "dialog.todo"),
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ',')}") },

                new EditArray(EditArray.ArrayChangeType.Pop, "user.todos"),
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ',')}") },

                // Take item
                new EditArray(EditArray.ArrayChangeType.Take, "user.todos"),
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ',')}") },

                // Clear list
                new EditArray(EditArray.ArrayChangeType.Clear, "user.todos"),
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ',')}") },
            };

            await CreateFlow(dialog, convoState, userState)
            .Send("hi")
                .AssertReply("Please add an item to todos.")
            .Send("todo1")
                .AssertReply("Your todos: todo1")
                .AssertReply("Please add an item to todos.")
            .Send("todo2")
                .AssertReply("Your todos: todo1, todo2")
                .AssertReply("Enter a item to remove.")
            .Send("todo2")
                .AssertReply("Your todos: todo1")
                .AssertReply("Please add an item to todos.")
            .Send("todo3")
                .AssertReply("Please add an item to todos.")
            .Send("todo4")
                .AssertReply("Your todos: todo1, todo3, todo4")
                .AssertReply("Your todos: todo1, todo3")
                .AssertReply("Your todos: todo3")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_IfProperty()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(new NoneIntentRule(
                    new List<IDialog>()
                    {
                        new IfCondition()
                        {
                            Condition = new ExpressionEngine().Parse("user.name == null"),
                            Steps = new List<IDialog>()
                            {
                                new TextInput() {
                                    Prompt = new ActivityTemplate("Hello, what is your name?"),
                                    Property = "user.name"
                                },
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    }));

            await CreateFlow(ruleDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_TextInput()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest")
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
                                Property = "user.name"
                            }
                        }
                    },
                    new SendActivity("Hello {user.name}, nice to meet you!")
                }
            };

            await CreateFlow(ruleDialog, convoState, userState)
                .Send("hi")
                    .AssertReply("Hello, what is your name?")
                .Send("Carlos")
                    .AssertReply("Hello Carlos, nice to meet you!")
                .StartTestAsync();
        }



        [TestMethod]
        public async Task AdaptiveDialog_StringLiteralInExpression()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Rules = new List<IRule>()
                {
                    new NoneIntentRule()
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
                                        OutputProperty = "user.name"
                                    }
                                }
                            },
                            new IfCondition()
                            {
                                // Check comparison with string literal
                                Condition = new ExpressionEngine().Parse("user.name == 'Carlos'"),
                                Steps = new List<IDialog>()
                                {
                                    new SendActivity("Hello carlin")
                                }
                            },
                            new SendActivity("Hello {user.name}, nice to meet you!")
                        }
                    }
                }
            };

            await CreateFlow(ruleDialog, convoState, userState)
            .Send(new Activity() { Type = ActivityTypes.ConversationUpdate, MembersAdded = new List<ChannelAccount>() { new ChannelAccount("bot", "Bot"), new ChannelAccount("user", "User") } })
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello carlin")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }


        [TestMethod]
        public async Task AdaptiveDialog_DoSteps()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        { "JokeIntent", "joke" },
                        { "HelloIntent", "hi|hello" }
                    }
                },
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
                                    Property = "user.name"
                                }
                            }
                    },
                    new SendActivity("Hello {user.name}, nice to meet you!")
                },
                Rules = new List<IRule>()
                {
                    new IntentRule()
                    {
                        Intent="JokeIntent",
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Why did the chicken cross the road?"),
                            new EndTurn(),
                            new SendActivity("To get to the other side")
                        }
                    },
                    new IntentRule()
                    {
                        Intent="HelloIntent",
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Hello {user.name}, nice to meet you!")
                        }
                    }
                },
            };

            await CreateFlow(ruleDialog, convoState, userState)
               .SendConversationUpdate()
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
        public async Task AdaptiveDialog_ReplacePlan()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");
            ruleDialog.AutoEndDialog = false;
            ruleDialog.Recognizer = new RegexRecognizer()
            {
                Intents = new Dictionary<string, string>()
                {
                    { "JokeIntent", "(?i)joke" },
                    { "GreetingIntent", "(?i)greeting|hi|hello" }
                }
            };

            ruleDialog.AddRules(new List<IRule>()
            {
                new BeginDialogRule(
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
                                    Property = "user.name"
                                }
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    }),
                new IntentRule()
                {
                    Intent= "GreetingIntent",
                    Steps = new List<IDialog>()
                    {
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    }
                },
                new IntentRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new EndTurn(),
                        new SendActivity("To get to the other side")
                    }),
                new NoneIntentRule(
                    steps: new List<IDialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                    }),
            });

            await CreateFlow(ruleDialog, convoState, userState)
            .SendConversationUpdate()
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
        public async Task AdaptiveDialog_NestedInlineSequences()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        { "JokeIntent", "joke"},
                        { "GreetingIntemt", "hi|hello"},
                        { "GoodbyeIntent", "bye|goodbye|seeya|see ya"},
                    }
                },
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
                                Property = "user.name"
                            }
                        }
                    },
                    new SendActivity("Hello {user.name}, nice to meet you!"),
                },
                Rules = new List<IRule>()
                {
                    new IntentRule("GreetingIntemt",
                        steps: new List<IDialog>()
                        {
                            new SendActivity("Hello {user.name}, nice to meet you!"),
                        }),
                    new IntentRule("JokeIntent",
                        steps: new List<IDialog>()
                        {
                            new SendActivity("Why did the chicken cross the road?"),
                            new EndTurn(),
                            new SendActivity("To get to the other side")
                        }),
                    new IntentRule("GoodbyeIntent",
                        steps: new List<IDialog>()
                        {
                            new SendActivity("See you later aligator!"),
                            new EndDialog()
                        }),
                    new NoneIntentRule(
                        new List<IDialog>()
                        {
                            new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                        })
                }
            };

            await CreateFlow(ruleDialog, convoState, userState)
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
            .Send("ummm")
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
            .Send("Goodbye")
                .AssertReply("See you later aligator!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_BeginDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var innerDialog = new AdaptiveDialog("innerDialog")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        { "JokeIntent", "(?i)joke"},
                        { "GreetingIntent", "(?i)hi|hello"},
                        { "GoodbyeIntent", "(?i)bye|goodbye|seeya|see ya"}
                    }
                },
                Rules = new List<IRule>()
                {
                    new BeginDialogRule()
                    {
                        Steps = new List<IDialog>()
                        {
                            new BeginDialog("Greeting"),
                            new SendActivity("I'm a joke bot. To get started say 'tell me a joke'"),
                        }
                    },

                    new IntentRule("JokeIntent",
                        steps: new List<IDialog>()
                        {
                            new BeginDialog("TellJokeDialog"),
                        }),

                    new IntentRule("GreetingIntent",
                        steps: new List<IDialog>()
                        {
                            new BeginDialog("Greeting"),
                        }),

                    new IntentRule("GoodbyeIntent",
                        steps: new List<IDialog>()
                        {
                            new SendActivity("See you later aligator!"),
                            new EndDialog()
                        }),

                    new NoneIntentRule(steps: new List<IDialog>()
                        {
                            new SendActivity("Like I said, I'm a joke bot. To get started say 'tell me a joke'"),
                        }),
                }
            };

            innerDialog.AddDialog(new[] {
                new AdaptiveDialog("Greeting")
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
                                    Property = "user.name"
                                },
                                new SendActivity("Hello {user.name}, nice to meet you!")
                            },
                            ElseSteps = new List<IDialog>()
                            {
                                new SendActivity("Hello {user.name}, nice to see you again!")
                            }
                        }
                    }
                }
            });

            innerDialog.AddDialog(new[] {
                new AdaptiveDialog("TellJokeDialog")
                    {
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Why did the chicken cross the road?"),
                            new EndTurn(),
                            new SendActivity("To get to the other side")
                        }
                    }
                });

            var outerDialog = new AdaptiveDialog("outer")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        { "BeginIntent", "(?i)begin" },
                        { "HelpIntent", "(?i)help" }
                    }
                },
                Steps = new List<IDialog>()
                {
                    new SendActivity("Hi, type 'begin' to start a dialog, type 'help' to get help.")
                },
                Rules = new List<IRule>()
                {
                    new IntentRule("BeginIntent")
                    {
                        Steps = new List<IDialog>()
                        {
                            new BeginDialog("innerDialog")
                        }
                    },
                    new IntentRule("HelpIntent")
                    {
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("help is coming")
                        }
                    },
                    new NoneIntentRule()
                    {
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Hi, type 'begin' to start a dialog, type 'help' to get help.")
                        }
                    },
                }
            };
            outerDialog.AddDialog(new List<IDialog>() { innerDialog });


            await CreateFlow(outerDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Hi, type 'begin' to start a dialog, type 'help' to get help.")
            .Send("begin")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
            .Send("tell me a joke")
                .AssertReply("Why did the chicken cross the road?")
            .Send("Why?")
                .AssertReply("To get to the other side")
            .Send("hi")
                .AssertReply("Hello Carlos, nice to see you again!")
            .Send("Do you know a joke?")
                .AssertReply("Why did the chicken cross the road?")
            .Send("Why?")
                .AssertReply("To get to the other side")
            .Send("ummm")
                 .AssertReply("Like I said, I'm a joke bot. To get started say 'tell me a joke'")
            .Send("help")
                .AssertReply("help is coming")
            .Send("bye")
                .AssertReply("See you later aligator!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_IntentRule()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        { "JokeIntent", "joke" }
                    }
                },
                Steps = new List<IDialog>()
                {
                    new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                },
                Rules = new List<IRule>()
                {
                    new IntentRule("JokeIntent",
                        steps: new List<IDialog>()
                        {
                            new SendActivity("Why did the chicken cross the road?"),
                            new EndTurn(),
                            new SendActivity("To get to the other side")
                        }),
                }
            };

            await CreateFlow(planningDialog, convoState, userState)
            .SendConversationUpdate()
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
            .Send("Do you know a joke?")
                .AssertReply("Why did the chicken cross the road?")
            .Send("Why?")
                .AssertReply("To get to the other side")
            .StartTestAsync();
        }


        //[TestMethod]
        //public async Task RuleSetTests()
        //{
        //    var convoState = new ConversationState(new MemoryStorage());
        //    var userState = new UserState(new MemoryStorage());

        //    var planningDialog = new AdaptiveDialog("planningTest")
        //    {
        //        Rules = new List<IRule>()
        //        {
        //            new RuleSet()
        //            {
        //                Constraint = "user.age > 13",
        //                Rules = new List<IRule>()
        //                {
        //                    new IntentRule()
        //                    {
        //                        Intent = "SubTest"
        //                    },
        //                    new RuleSet()
        //                    {
        //                        Rules = new List<IRule>()
        //                        {
        //                            new IntentRule()
        //                            {
        //                                Intent = "SubSubTest"
        //                            },
        //                        }
        //                    }
        //                }
        //            },
        //            new IntentRule()
        //            {
        //                Intent = "Test"
        //            }
        //        }
        //    };

        //    var rules = planningDialog.GetAllRules(planningDialog.Rules).ToList();
        //    Assert.AreEqual(3, rules.Count(), "rules collapsing failed");
        //}

    }
}
