// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class AdaptiveDialogTests
    {
        public TestContext TestContext { get; set; }

        public ExpressionEngine expressionEngine { get; set; } = new ExpressionEngine();

        private TestFlow CreateFlow(AdaptiveDialog ruleDialog)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();

            var explorer = new ResourceExplorer();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .Use(new RegisterClassMiddleware<ResourceExplorer>(explorer))
                .UseLanguageGeneration(explorer)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            DialogManager dm = new DialogManager(ruleDialog);
            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }

        [TestMethod]
        public async Task AdaptiveDialog_TopLevelFallback()
        {
            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new SendActivity("Hello Planning!")
                    }));

            await CreateFlow(ruleDialog)
            .Send("start")
                .AssertReply("Hello Planning!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_TopLevelFallbackMultipleActivities()
        {
            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(new UnknownIntentRule(new List<IDialog>()
                    {
                        new SendActivity("Hello Planning!"),
                        new SendActivity("Howdy awain")
                    }));

            await CreateFlow(ruleDialog)
            .Send("start")
                .AssertReply("Hello Planning!")
                .AssertReply("Howdy awain")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_EndTurn()
        {
            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(
                new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new TextInput()
                        {
                            Prompt = new ActivityTemplate("Hello, what is your name?"),
                            Property = "user.name"
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!"),
                    }));

            await CreateFlow(ruleDialog)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_EditArray()
        {
            var dialog = new AdaptiveDialog("planningTest");
            dialog.Steps = new List<IDialog>()
            {
                // Add item
                new TextInput()
                {
                    AlwaysPrompt = true,
                    Prompt = new ActivityTemplate("Please add an item to todos."),
                    Property = "dialog.todo"
                },
                new InitProperty() { Property = "user.todos", Type = "array" },
                new EditArray(EditArray.ArrayChangeType.Push, "user.todos", "dialog.todo"),
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ', ')}") },
                new TextInput()
                {
                    AlwaysPrompt = true,
                    Prompt = new ActivityTemplate("Please add an item to todos."),
                    Property = "dialog.todo"
                },
                new EditArray(EditArray.ArrayChangeType.Push, "user.todos", "dialog.todo"),
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ', ')}") },

                // Remove item
                new TextInput()
                {
                    AlwaysPrompt = true,
                    Prompt = new ActivityTemplate("Enter a item to remove."),
                    Property = "dialog.todo"
                },
                new EditArray(EditArray.ArrayChangeType.Remove, "user.todos", "dialog.todo"),
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ', ')}") },

                // Add item and pop item
                new TextInput()
                {
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
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ', ')}") },

                new EditArray(EditArray.ArrayChangeType.Pop, "user.todos"),
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ', ')}") },

                // Take item
                new EditArray(EditArray.ArrayChangeType.Take, "user.todos"),
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ', ')}") },

                // Clear list
                new EditArray(EditArray.ArrayChangeType.Clear, "user.todos"),
                new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ', ')}") },
            };

            await CreateFlow(dialog)
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
            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(new UnknownIntentRule(
                    new List<IDialog>()
                    {
                        new IfCondition()
                        {
                            Condition = "user.name == null",
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

            await CreateFlow(ruleDialog)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_TextInput()
        {
            var ruleDialog = new AdaptiveDialog("planningTest")
            {
                Steps = new List<IDialog>()
                {
                    new IfCondition()
                    {
                        Condition = "user.name == null",
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

            await CreateFlow(ruleDialog)
                .Send("hi")
                    .AssertReply("Hello, what is your name?")
                .Send("Carlos")
                    .AssertReply("Hello Carlos, nice to meet you!")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_StringLiteralInExpression()
        {
            var ruleDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Rules = new List<IRule>()
                {
                    new UnknownIntentRule()
                    {
                        Steps = new List<IDialog>()
                        {
                            new IfCondition()
                            {
                                Condition = "user.name == null",
                                Steps = new List<IDialog>()
                                {
                                    new TextInput()
                                    {
                                        Prompt = new ActivityTemplate("Hello, what is your name?"),
                                        OutputBinding = "user.name"
                                    }
                                }
                            },
                            new IfCondition()
                            {
                                // Check comparison with string literal
                                Condition = "user.name == 'Carlos'",
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

            await CreateFlow(ruleDialog)
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
                        Condition = "user.name == null",
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
                        Intent = "JokeIntent",
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Why did the chicken cross the road?"),
                            new EndTurn(),
                            new SendActivity("To get to the other side")
                        }
                    },
                    new IntentRule()
                    {
                        Intent = "HelloIntent",
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Hello {user.name}, nice to meet you!")
                        }
                    }
                },
            };

            await CreateFlow(ruleDialog)
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
            ruleDialog.Steps = new List<IDialog>()
                    {
                        new IfCondition()
                        {
                            Condition = "user.name == null",
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
                    };

            ruleDialog.AddRules(new List<IRule>()
            {
                new IntentRule()
                {
                    Intent = "GreetingIntent",
                    Steps = new List<IDialog>()
                    {
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    }
                },
                new IntentRule(
                    "JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new EndTurn(),
                        new SendActivity("To get to the other side")
                    }),
                new UnknownIntentRule(
                    steps: new List<IDialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                    })
            });

            await CreateFlow(ruleDialog)
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
                        Condition = "user.name == null",
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
                    new UnknownIntentRule(
                        new List<IDialog>()
                        {
                            new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                        })
                }
            };

            await CreateFlow(ruleDialog)
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
                Steps = new List<IDialog>()
                {
                    new BeginDialog("Greeting"),
                    new SendActivity("I'm a joke bot. To get started say 'tell me a joke'"),
                },
                Rules = new List<IRule>()
                {
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

                    new UnknownIntentRule(steps: new List<IDialog>()
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
                            Condition = "user.name == null",
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
                },
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
                    new UnknownIntentRule()
                    {
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Hi, type 'begin' to start a dialog, type 'help' to get help.")
                        }
                    },
                }
            };
            outerDialog.AddDialog(new List<IDialog>() { innerDialog });


            await CreateFlow(outerDialog)
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

            await CreateFlow(planningDialog)
            .SendConversationUpdate()
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
            .Send("Do you know a joke?")
                .AssertReply("Why did the chicken cross the road?")
            .Send("Why?")
                .AssertReply("To get to the other side")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_NestedRecognizers()
        {
            var outerDialog = new AdaptiveDialog("outer")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        { "SideIntent", "side" },
                        { "CancelIntent", "cancel" },
                    }
                },
                Steps = new List<IDialog>()
                {
                    new TextInput()
                    {
                        Prompt = new ActivityTemplate("name?"),
                        Property = "user.name"
                    },
                    new SendActivity("{user.name}"),
                    new NumberInput()
                    {
                        Prompt = new ActivityTemplate("age?"),
                        Property = "user.age"
                    },
                    new SendActivity("{user.age}"),
                },
                Rules = new List<IRule>()
                {
                    new IntentRule("SideIntent") { Steps = new List<IDialog>() { new SendActivity("sideintent") } },
                    new IntentRule("CancelIntent") { Steps = new List<IDialog>() { new EndDialog() } },
                    new UnknownIntentRule() { Steps = new List<IDialog>() { new SendActivity("outerWhat") } }
                }
            };

            var ruleDialog = new AdaptiveDialog("root")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        { "StartOuterIntent", "start" },
                        { "RootIntent", "root" },
                    }
                },
                Rules = new List<IRule>()
                {
                    new IntentRule("StartOuterIntent", steps: new List<IDialog>() { outerDialog }),
                    new IntentRule("RootIntent", steps: new List<IDialog>() { new SendActivity("rootintent") }),
                    new UnknownIntentRule( new List<IDialog>() { new SendActivity("rootunknown") })
                }
            };

            await CreateFlow(ruleDialog)
            .Send("start")
                .AssertReply("name?")
            .Send("side")
                .AssertReply("sideintent")
                .AssertReply("name?")
            .Send("root")
                .AssertReply("rootintent")
                .AssertReply("name?")
            .Send("Carlos")
                .AssertReply("Carlos")
                .AssertReply("age?")
            .Send("root")
                .AssertReply("rootintent")
                .AssertReply("age?")
            .Send("side")
                .AssertReply("sideintent")
                .AssertReply("age?")
            .Send("10")
                .AssertReply("10")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_ActivityRules()
        {
            var dialog = new AdaptiveDialog("test")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        { "JokeIntent", "joke" }
                    }
                },
                Rules = new List<IRule>()
                {
                    new ActivityRule("Custom", steps: new List<IDialog>() { new SendActivity("CustomActivityRule") }),
                    new MessageActivityRule(steps: new List<IDialog>() { new SendActivity("MessageActivityRule") }),
                    new MessageDeleteActivityRule(steps: new List<IDialog>() { new SendActivity("MessageDeleteActivityRule") }),
                    new MessageUpdateActivityRule(steps: new List<IDialog>() { new SendActivity("MessageUpdateActivityRule") }),
                    new MessageReactionActivityRule(steps: new List<IDialog>() { new SendActivity("MessageReactionActivityRule") }),
                    new ConversationUpdateActivityRule(steps: new List<IDialog>() { new SendActivity("ConversationUpdateActivityRule") }),
                    new EndOfConversationActivityRule(steps: new List<IDialog>() { new SendActivity("EndOfConversationActivityRule") }),
                    new InvokeActivityRule(steps: new List<IDialog>() { new SendActivity("InvokeActivityRule") }),
                    new EventActivityRule(steps: new List<IDialog>() { new SendActivity("EventActivityRule") }),
                    new HandoffActivityRule(steps: new List<IDialog>() { new SendActivity("HandoffActivityRule") }),
                    new TypingActivityRule(steps: new List<IDialog>() { new SendActivity("TypingActivityRule") }),
                    new MessageActivityRule(constraint: "turn.activity.text == 'constraint'", steps: new List<IDialog>() { new SendActivity("constraint") }),
                }
            };

            await CreateFlow(dialog)
            .SendConversationUpdate()
                .AssertReply("ConversationUpdateActivityRule")
            .Send("MessageActivityRule")
                .AssertReply("MessageActivityRule")
            .Send("constraint")
                .AssertReply("constraint")
            .Send(new Activity(type: ActivityTypes.MessageUpdate))
                .AssertReply("MessageUpdateActivityRule")
            .Send(new Activity(type: ActivityTypes.MessageDelete))
                .AssertReply("MessageDeleteActivityRule")
            .Send(new Activity(type: ActivityTypes.MessageReaction))
                .AssertReply("MessageReactionActivityRule")
            .Send(Activity.CreateTypingActivity())
                .AssertReply("TypingActivityRule")
            .Send(Activity.CreateEndOfConversationActivity())
                .AssertReply("EndOfConversationActivityRule")
            .Send(Activity.CreateEventActivity())
                .AssertReply("EventActivityRule")
            .Send(Activity.CreateHandoffActivity())
                .AssertReply("HandoffActivityRule")
            .Send(Activity.CreateInvokeActivity())
                .AssertReply("InvokeActivityRule")
            .Send(new Activity(type: "Custom"))
                .AssertReply("CustomActivityRule")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_ActivityAndIntentRules()
        {
            var dialog = new AdaptiveDialog("test")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        { "JokeIntent", "joke" }
                    }
                },
                Rules = new List<IRule>()
                {
                    new IntentRule(intent: "JokeIntent", steps: new List<IDialog>() { new SendActivity("chicken joke") }),
                    new MessageActivityRule(constraint: "turn.activity.text == 'magic'", steps: new List<IDialog>() { new SendActivity("abracadabra") }),
                }
            };

            await CreateFlow(dialog)
            .Send("tell me a joke")
                .AssertReply("chicken joke")
            .Send("magic")
                .AssertReply("abracadabra")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_BindingCaptureValueWithinSameAdaptive()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Rules = new List<IRule>()
                {
                    new UnknownIntentRule()
                    {
                        Steps = new List<IDialog>()
                        {
                            new NumberInput()
                            {
                                Property = "$number",
                                Prompt = new ActivityTemplate("Give me a number")
                            },
                            new SendActivity()
                            {
                                Activity = new ActivityTemplate("You said {$number}")
                            }
                        }
                    }
                }
            };

            await CreateFlow(rootDialog)
            .Send("hi")
                .AssertReply("Give me a number")
            .Send("32")
                .AssertReply("You said 32")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_BindingReferValueInNestedStep()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Rules = new List<IRule>()
                {
                    new UnknownIntentRule()
                    {
                        Steps = new List<IDialog>()
                        {
                            new NumberInput()
                            {
                                Property = "$age",
                                Prompt = new ActivityTemplate("Hello, how old are you?")
                            },
                            new IfCondition()
                            {
                                Condition = "$age > 80",
                                Steps = new List<IDialog>()
                                {
                                    new SendActivity("Thanks, you are quite young!")
                                },
                                ElseSteps = new List<IDialog>()
                                {
                                    new SendActivity("Thanks, you are awesome!")
                                }
                            }
                        }
                    }
                }
            };

            await CreateFlow(rootDialog)
            .Send("Hi")
                .AssertReply("Hello, how old are you?")
            .Send("94")
                .AssertReply("Thanks, you are quite young!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_BindingOptionsAcrossAdaptiveDialogs()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Rules = new List<IRule>()
                {
                    new UnknownIntentRule()
                    {
                        Steps = new List<IDialog>()
                        {
                            new NumberInput()
                            {
                                Property = "$age",
                                Prompt = new ActivityTemplate("Hello, how old are you?")
                            },
                            new BeginDialog("ageDialog")
                            {
                                Options = new
                                {
                                    userAge = "$age"
                                }
                            }
                        }
                    }
                }
            };

            var ageDialog = new AdaptiveDialog("ageDialog")
            {
                Rules = new List<IRule>()
                {
                    new UnknownIntentRule()
                    {
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Hello, you are {dialog.options.userAge} years old!"),
                            new SendActivity("And your actual age is {$options.userAge}")
                        }
                    }
                }
            };

            rootDialog.AddDialog(ageDialog);

            await CreateFlow(rootDialog)
            .Send("Hi")
                .AssertReply("Hello, how old are you?")
            .Send("44")
                .AssertReply("Hello, you are 44 years old!")
                .AssertReply("And your actual age is 44")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_BindingReferValueInLaterStep()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Rules = new List<IRule>()
                {
                    new UnknownIntentRule()
                    {
                        Steps = new List<IDialog>()
                        {
                            new TextInput()
                            {
                                Property = "$name",
                                Prompt = new ActivityTemplate("What is your name?")
                            },
                            new NumberInput()
                            {
                                Property = "$age",
                                Prompt = new ActivityTemplate("Hello {$name}, how old are you?")
                            },
                            new SendActivity()
                            {
                                Activity = new ActivityTemplate("Hello {$name}, I have your age as {$age}")
                            }
                        }
                    }
                }
            };

            await CreateFlow(rootDialog)
            .Send("Hi")
                .AssertReply("What is your name?")
            .Send("zoidberg")
                .AssertReply("Hello zoidberg, how old are you?")
            .Send("22")
                .AssertReply("Hello zoidberg, I have your age as 22")
            .StartTestAsync();
        }
      
        [TestMethod]
        public async Task AdaptiveDialog_BindingTwoWayAcrossAdaptiveDialogs_AnonymousOptions()
        {
            await TestBindingTwoWayAcrossAdaptiveDialogs(new { userName = "$name" });
        }

        [TestMethod]
        public async Task AdaptiveDialog_BindingTwoWayAcrossAdaptiveDialogs_ObjectDictionaryOptions()
        {
            await TestBindingTwoWayAcrossAdaptiveDialogs(new Dictionary<string, object>() { { "userName", "$name" } });
        }

        [TestMethod]
        public async Task AdaptiveDialog_BindingTwoWayAcrossAdaptiveDialogs_StringDictionaryOptions()
        {
            await TestBindingTwoWayAcrossAdaptiveDialogs(new Dictionary<string, string>() { { "userName", "$name" } });
        }

        class Person
        {
            public string userName { get; set; }
        }

        [TestMethod]
        public async Task AdaptiveDialog_BindingTwoWayAcrossAdaptiveDialogs_StronglyTypedOptions()
        {
            await TestBindingTwoWayAcrossAdaptiveDialogs(new Person() { userName = "$name" });
        }

        public async Task TestBindingTwoWayAcrossAdaptiveDialogs(object options)
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Rules = new List<IRule>()
                {
                    new UnknownIntentRule()
                    {
                        Steps = new List<IDialog>()
                        {
                            new TextInput()
                            {
                                Property = "$name",
                                Prompt = new ActivityTemplate("Hello, what is your name?")
                            },
                            new BeginDialog("ageDialog")
                            {
                                Options = options,
                                Property = "$age"
                            },
                            new SendActivity("Hello {$name}, you are {$age} years old!")
                        }
                    }
                }
            };

            var ageDialog = new AdaptiveDialog("ageDialog")
            {
                Rules = new List<IRule>()
                {
                    new UnknownIntentRule()
                    {
                        Steps = new List<IDialog>()
                        {
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("Hello {$options.userName}, how old are you?"),
                                Property = "$age"
                            },
                            new EndDialog()
                            {
                                ResultProperty = "$age"
                            }
                        }
                    }
                }
            };

            rootDialog.AddDialog(ageDialog);

            await CreateFlow(rootDialog)
            .Send("Hi")
                .AssertReply("Hello, what is your name?")
            .Send("zoidberg")
                .AssertReply("Hello zoidberg, how old are you?")
            .Send("I'm 77")
                .AssertReply("Hello zoidberg, you are 77 years old!")
            .StartTestAsync();
        }
    }
}
