// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.LanguageGeneration.Generators;
using Microsoft.Bot.Builder.LanguageGeneration.Templates;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class AdaptiveDialogTests
    {
        public TestContext TestContext { get; set; }

        public ExpressionEngine ExpressionEngine { get; set; } = new ExpressionEngine();

        [TestMethod]
        public async Task AdaptiveDialog_TopLevelFallback()
        {
            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.Triggers.Add(new OnUnknownIntent(
                    new List<Dialog>()
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

            ruleDialog.Triggers.Add(new OnUnknownIntent(new List<Dialog>()
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

            ruleDialog.Triggers.Add(
                new OnUnknownIntent(
                    new List<Dialog>()
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
            dialog.Triggers.Add(new OnBeginDialog()
            {
                Actions = new List<Dialog>()
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
                    new TextInput()
                    {
                        AlwaysPrompt = true,
                        Prompt = new ActivityTemplate("Enter a item to remove."),
                        Property = "dialog.todo"
                    },
                    new EditArray(EditArray.ArrayChangeType.Remove, "user.todos", "dialog.todo"),
                    new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ',')}") },

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
                    new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ',')}") },

                    new EditArray(EditArray.ArrayChangeType.Pop, "user.todos"),
                    new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ',')}") },

                    // Take item
                    new EditArray(EditArray.ArrayChangeType.Take, "user.todos"),
                    new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ',')}") },

                    // Clear list
                    new EditArray(EditArray.ArrayChangeType.Clear, "user.todos"),
                    new SendActivity() { Activity = new ActivityTemplate("Your todos: {join(user.todos, ',')}") },
                }
            });

            await CreateFlow(dialog)
            .Send("hi")
                .AssertReply("Please add an item to todos.")
            .Send("todo1")
                .AssertReply("Your todos: todo1")
                .AssertReply("Please add an item to todos.")
            .Send("todo2")
                .AssertReply("Your todos: todo1,todo2")
                .AssertReply("Enter a item to remove.")
            .Send("todo2")
                .AssertReply("Your todos: todo1")
                .AssertReply("Please add an item to todos.")
            .Send("todo3")
                .AssertReply("Please add an item to todos.")
            .Send("todo4")
                .AssertReply("Your todos: todo1,todo3,todo4")
                .AssertReply("Your todos: todo1,todo3")
                .AssertReply("Your todos: todo3")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_IfProperty()
        {
            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.Triggers.Add(new OnUnknownIntent(
                    new List<Dialog>()
                    {
                        new IfCondition()
                        {
                            Condition = "user.name == null",
                            Actions = new List<Dialog>()
                            {
                                new TextInput()
                                {
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
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new IfCondition()
                            {
                                Condition = "user.name == null",
                                Actions = new List<Dialog>()
                                {
                                    new TextInput()
                                    {
                                        Prompt = new ActivityTemplate("Hello, what is your name?"),
                                        Property = "user.name",
                                        AllowInterruptions = AllowInterruptions.Never
                                    }
                                }
                            },
                            new SendActivity("Hello {user.name}, nice to meet you!")
                        }
                    }
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
                Triggers = new List<OnCondition>()
                {
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
                        {
                            new IfCondition()
                            {
                                Condition = "user.name == null",
                                Actions = new List<Dialog>()
                                {
                                    new TextInput()
                                    {
                                        Prompt = new ActivityTemplate("Hello, what is your name?"),
                                        Property = "user.name"
                                    }
                                }
                            },
                            new IfCondition()
                            {
                                // Check comparison with string literal
                                Condition = "user.name == 'Carlos'",
                                Actions = new List<Dialog>()
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
        public async Task AdaptiveDialog_TextInputDefaultValueResponse()
        {
            var ruleDialog = new AdaptiveDialog("planningTest")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("Hello, what is your age?"),
                                Property = "user.age",
                                DefaultValue = "10",
                                MaxTurnCount = 2,
                                DefaultValueResponse = new ActivityTemplate("I am going to say you are 10.")
                            },
                            new SendActivity()
                            {
                                Activity = new ActivityTemplate("Your age is {user.age}.")
                            }
                        }
                    }
                }
            };

            await CreateFlow(ruleDialog)
                .SendConversationUpdate()
                    .AssertReply("Hello, what is your age?")
                .Send("Why do you want to know")
                    .AssertReply("Hello, what is your age?")
                .Send("Why do you want to know")
                    .AssertReply("I am going to say you are 10.")
                    .AssertReply("Your age is 10.")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_TextInputNoMaxTurnCount()
        {
            var ruleDialog = new AdaptiveDialog("planningTest")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("Hello, what is your age?"),
                                Property = "user.age",
                                DefaultValue = "10",
                                DefaultValueResponse = new ActivityTemplate("I am going to say you are 10.")
                            },
                            new SendActivity()
                            {
                                Activity = new ActivityTemplate("Your age is {user.age}.")
                            }
                        }
                    }
                }
            };

            await CreateFlow(ruleDialog)
                .SendConversationUpdate()
                    .AssertReply("Hello, what is your age?")
                .Send("Why do you want to know")
                    .AssertReply("Hello, what is your age?")
                .Send("Why do you want to know")
                    .AssertReply("Hello, what is your age?")
                .Send("Why do you want to know")
                    .AssertReply("Hello, what is your age?")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_DoActions()
        {
            var ruleDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("JokeIntent", "joke"),
                        new IntentPattern("HelloIntent", "hi|hello"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new IfCondition()
                            {
                                Condition = "user.name == null",
                                Actions = new List<Dialog>()
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
                    },
                    new OnIntent()
                    {
                        Intent = "JokeIntent",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Why did the chicken cross the road?"),
                            new EndTurn(),
                            new SendActivity("To get to the other side")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "HelloIntent",
                        Actions = new List<Dialog>()
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
                Intents = new List<IntentPattern>()
                {
                    new IntentPattern("JokeIntent", "(?i)joke"),
                    new IntentPattern("GreetingIntent", "(?i)greeting|hi|hello"),
                }
            };

            ruleDialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnBeginDialog()
                {
                    Actions = new List<Dialog>()
                    {
                        new IfCondition()
                        {
                            Condition = "user.name == null",
                            Actions = new List<Dialog>()
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
                },
                new OnIntent()
                {
                    Intent = "GreetingIntent",
                    Actions = new List<Dialog>()
                    {
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    }
                },
                new OnIntent(
                    "JokeIntent",
                    actions: new List<Dialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new EndTurn(),
                        new SendActivity("To get to the other side")
                    }),
                new OnUnknownIntent(
                    actions: new List<Dialog>()
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
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("JokeIntent", "joke"),
                        new IntentPattern("GreetingIntemt", "hi|hello"),
                        new IntentPattern("GoodbyeIntent", "bye|goodbye|seeya|see ya"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new IfCondition()
                            {
                                Condition = "user.name == null",
                                Actions = new List<Dialog>()
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
                    },
                    new OnIntent(
                        "GreetingIntemt",
                        actions: new List<Dialog>()
                        {
                            new SendActivity("Hello {user.name}, nice to meet you!"),
                        }),
                    new OnIntent(
                        "JokeIntent",
                        actions: new List<Dialog>()
                        {
                            new SendActivity("Why did the chicken cross the road?"),
                            new EndTurn(),
                            new SendActivity("To get to the other side")
                        }),
                    new OnIntent(
                        "GoodbyeIntent",
                        actions: new List<Dialog>()
                        {
                            new SendActivity("See you later aligator!"),
                            new EndDialog()
                        }),
                    new OnUnknownIntent(
                        new List<Dialog>()
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
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("JokeIntent", "(?i)joke"),
                        new IntentPattern("GreetingIntent", "(?i)hi|hello"),
                        new IntentPattern("GoodbyeIntent", "(?i)bye|goodbye|seeya|see ya"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog("Greeting"),
                            new SendActivity("I'm a joke bot. To get started say 'tell me a joke'"),
                        },
                    },

                    new OnIntent(
                        "JokeIntent",
                        actions: new List<Dialog>()
                        {
                            new BeginDialog("TellJokeDialog"),
                        }),

                    new OnIntent(
                        "GreetingIntent",
                        actions: new List<Dialog>()
                        {
                            new BeginDialog("Greeting"),
                        }),

                    new OnIntent(
                        "GoodbyeIntent",
                        actions: new List<Dialog>()
                        {
                            new SendActivity("See you later aligator!"),
                            new EndDialog()
                        }),

                    new OnUnknownIntent(actions: new List<Dialog>()
                        {
                            new SendActivity("Like I said, I'm a joke bot. To get started say 'tell me a joke'"),
                        }),
                }
            };

            innerDialog.Dialogs.Add(
                new AdaptiveDialog("Greeting")
                {
                    Triggers = new List<OnCondition>()
                    {
                        new OnBeginDialog()
                        {
                            Actions = new List<Dialog>()
                            {
                                new IfCondition()
                                {
                                    Condition = "user.name == null",
                                    Actions = new List<Dialog>()
                                    {
                                        new TextInput()
                                        {
                                            Prompt = new ActivityTemplate("Hello, what is your name?"),
                                            Property = "user.name"
                                        },
                                        new SendActivity("Hello {user.name}, nice to meet you!")
                                    },
                                    ElseActions = new List<Dialog>()
                                    {
                                        new SendActivity("Hello {user.name}, nice to see you again!")
                                    }
                                }
                            }
                        }
                    }
                });
            innerDialog.Dialogs.Add(
                new AdaptiveDialog("TellJokeDialog")
                {
                    Triggers = new List<OnCondition>()
                    {
                        new OnBeginDialog()
                        {
                            Actions = new List<Dialog>()
                            {
                                new SendActivity("Why did the chicken cross the road?"),
                                new EndTurn(),
                                new SendActivity("To get to the other side")
                            }
                        }
                    }
                });

            var outerDialog = new AdaptiveDialog("outer")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("BeginIntent", "(?i)begin"),
                        new IntentPattern("HelpIntent", "(?i)help"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Hi, type 'begin' to start a dialog, type 'help' to get help.")
                        },
                    },
                    new OnIntent("BeginIntent")
                    {
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog("innerDialog")
                        }
                    },
                    new OnIntent("HelpIntent")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("help is coming")
                        }
                    },
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Hi, type 'begin' to start a dialog, type 'help' to get help.")
                        }
                    },
                }
            };
            outerDialog.Dialogs.Add(innerDialog);

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
        public async Task AdaptiveDialog_NestedRecognizers()
        {
            var outerDialog = new AdaptiveDialog("outer")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("SideIntent", "side"),
                        new IntentPattern("CancelIntent", "cancel"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
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
                                Property = "user.age",
                                AllowInterruptions = AllowInterruptions.Never,
                                MaxTurnCount = 2,
                            },
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("age?"),
                                Property = "user.age",
                                AllowInterruptions = AllowInterruptions.Always,
                            },
                            new SendActivity("{user.age}")
                        }
                    },
                    new OnIntent("SideIntent") { Actions = new List<Dialog>() { new SendActivity("sideintent") } },
                    new OnIntent("CancelIntent") { Actions = new List<Dialog>() { new EndDialog() } },
                    new OnUnknownIntent() { Actions = new List<Dialog>() { new SendActivity("outerWhat") } }
                }
            };

            var ruleDialog = new AdaptiveDialog("root")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("StartOuterIntent", "start"),
                        new IntentPattern("RootIntent", "root"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent("StartOuterIntent", actions: new List<Dialog>() { outerDialog }),
                    new OnIntent("RootIntent", actions: new List<Dialog>() { new SendActivity("rootintent") }),
                    new OnUnknownIntent(new List<Dialog>() { new SendActivity("rootunknown") })
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
                .AssertReply("age?") // turnCount = 1
            .Send("root") // allowInterruptions = never
                .AssertReply("age?") // turnCount = 2
            .Send("side") // fail to recognize and end
                .AssertReply("age?") // new NumberInput with allowInterruptions = always
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
        public async Task AdaptiveDialog_ActivityEvents()
        {
            var dialog = new AdaptiveDialog("test")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("JokeIntent", "joke"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnActivity("Custom", actions: new List<Dialog>() { new SendActivity("CustomActivityEvent") }),
                    new OnMessageActivity(actions: new List<Dialog>() { new SendActivity("MessageActivityEvent") }),
                    new OnMessageDeleteActivity(actions: new List<Dialog>() { new SendActivity("MessageDeleteActivityEvent") }),
                    new OnMessageUpdateActivity(actions: new List<Dialog>() { new SendActivity("MessageUpdateActivityEvent") }),
                    new OnMessageReactionActivity(actions: new List<Dialog>() { new SendActivity("MessageReactionActivityEvent") }),
                    new OnConversationUpdateActivity(actions: new List<Dialog>() { new SendActivity("ConversationUpdateActivityEvent") }),
                    new OnEndOfConversationActivity(actions: new List<Dialog>() { new SendActivity("EndOfConversationActivityEvent") }),
                    new OnInvokeActivity(actions: new List<Dialog>() { new SendActivity("InvokeActivityEvent") }),
                    new OnEventActivity(actions: new List<Dialog>() { new SendActivity("EventActivityEvent") }),
                    new OnHandoffActivity(actions: new List<Dialog>() { new SendActivity("HandoffActivityEvent") }),
                    new OnTypingActivity(actions: new List<Dialog>() { new SendActivity("TypingActivityEvent") }),
                    new OnMessageActivity(constraint: "turn.activity.text == 'constraint'", actions: new List<Dialog>() { new SendActivity("constraint") }),
                }
            };

            await CreateFlow(dialog)
            .SendConversationUpdate()
                .AssertReply("ConversationUpdateActivityEvent")
            .Send("MessageActivityEvent")
                .AssertReply("MessageActivityEvent")
            .Send("constraint")
                .AssertReply("constraint")
            .Send(new Activity(type: ActivityTypes.MessageUpdate))
                .AssertReply("MessageUpdateActivityEvent")
            .Send(new Activity(type: ActivityTypes.MessageDelete))
                .AssertReply("MessageDeleteActivityEvent")
            .Send(new Activity(type: ActivityTypes.MessageReaction))
                .AssertReply("MessageReactionActivityEvent")
            .Send(Activity.CreateTypingActivity())
                .AssertReply("TypingActivityEvent")
            .Send(Activity.CreateEndOfConversationActivity())
                .AssertReply("EndOfConversationActivityEvent")
            .Send(Activity.CreateEventActivity())
                .AssertReply("EventActivityEvent")
            .Send(Activity.CreateHandoffActivity())
                .AssertReply("HandoffActivityEvent")
            .Send(Activity.CreateInvokeActivity())
                .AssertReply("InvokeActivityEvent")
            .Send(new Activity(type: "Custom"))
                .AssertReply("CustomActivityEvent")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_ActivityAndIntentEvents()
        {
            var dialog = new AdaptiveDialog("test")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("JokeIntent", "joke"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent(intent: "JokeIntent", actions: new List<Dialog>() { new SendActivity("chicken joke") }),
                    new OnMessageActivity(constraint: "turn.activity.text == 'magic'", actions: new List<Dialog>() { new SendActivity("abracadabra") }),
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
                Triggers = new List<OnCondition>()
                {
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
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
        public async Task AdaptiveDialog_BindingReferValueInNestedAction()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Triggers = new List<OnCondition>()
                {
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
                        {
                            new NumberInput()
                            {
                                Property = "$age",
                                Prompt = new ActivityTemplate("Hello, how old are you?")
                            },
                            new IfCondition()
                            {
                                Condition = "$age > 80",
                                Actions = new List<Dialog>()
                                {
                                    new SendActivity("Thanks, you are quite young!")
                                },
                                ElseActions = new List<Dialog>()
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
                Triggers = new List<OnCondition>()
                {
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
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
                Triggers = new List<OnCondition>()
                {
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Hello, you are {dialog.options.userAge} years old!"),
                            new SendActivity("And your actual age is {$options.userAge}")
                        }
                    }
                }
            };

            rootDialog.Dialogs.Add(ageDialog);

            await CreateFlow(rootDialog)
            .Send("Hi")
                .AssertReply("Hello, how old are you?")
            .Send("44")
                .AssertReply("Hello, you are 44 years old!")
                .AssertReply("And your actual age is 44")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_BindingReferValueInLaterAction()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Triggers = new List<OnCondition>()
                {
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
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

        [TestMethod]
        public async Task AdaptiveDialog_BindingTwoWayAcrossAdaptiveDialogs_StronglyTypedOptions()
        {
            await TestBindingTwoWayAcrossAdaptiveDialogs(new Person() { UserName = "$name" });
        }

        [TestMethod]
        public async Task AdaptiveDialog_PropertySetInInterruption()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("Interruption", "(?i)interrupt"),
                        new IntentPattern("Greeting", "(?i)hi"),
                        new IntentPattern("Start", "(?i)start"),
                        new IntentPattern("noage", "(?i)no"),
                        new IntentPattern("why", "(?i)why"),
                        new IntentPattern("reset", "(?i)reset"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Hello, I'm the demo bot.")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "reset",
                        Actions = new List<Dialog>()
                        {
                            new DeleteProperty()
                            {
                                Property = "user.name"
                            },
                            new SendActivity("Sure. I've reset your profile.")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "Start",
                        Actions = new List<Dialog>()
                        {
                            new TextInput()
                            {
                                Prompt = new ActivityTemplate("What is your name?"),
                                Property = "user.name",
                                AllowInterruptions = AllowInterruptions.Always
                            },
                            new SendActivity("I have {user.name} as your name")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "Interruption",
                        Actions = new List<Dialog>()
                        {
                            // short circuiting Interruption so consultation is terminated. 
                            new SendActivity("In Interruption..."),

                            // request the active input step to re-process user input. 
                            new SetProperty()
                            {
                                Property = "turn.processInput",
                                Value = "true"
                            }
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "Greeting",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Hi, I'm the test bot!")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "noage",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Sure, no problem. I'll set your name to 'Human'. you can say reset to start over"),
                            new SetProperty()
                            {
                                Property = "user.name",
                                Value = "'Human'"
                            }
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "why",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("I need your name to be able to address you correctly")
                        }
                    }
                }
            };

            await CreateFlow(rootDialog)
            .Send("start")
                .AssertReply("What is your name?")
            .Send("why")
                .AssertReply("I need your name to be able to address you correctly")
                .AssertReply("What is your name?")
            .Send("hi")
                .AssertReply("Hi, I'm the test bot!")
                .AssertReply("What is your name?")
            .Send("reset")
                .AssertReply("Sure. I've reset your profile.")
                .AssertReply("What is your name?")
            .Send("no")
                .AssertReply("Sure, no problem. I'll set your name to 'Human'. you can say reset to start over")
                .AssertReply("I have Human as your name")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_ReProcessInputProperty()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("Interruption", "(?i)interrupt"),
                        new IntentPattern("Start", "(?i)start"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent()
                    {
                        Intent = "Start",
                        Actions = new List<Dialog>()
                        {
                            new TextInput()
                            {
                                Prompt = new ActivityTemplate("What is your name?"),
                                Property = "user.name",
                                AllowInterruptions = AllowInterruptions.Always
                            },
                            new SendActivity("I have {user.name} as your name")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "Interruption",
                        Actions = new List<Dialog>()
                        {
                            // short circuiting Interruption so consultation is terminated. 
                            new SendActivity("In Interruption..."),

                            // request the active input step to re-process user input. 
                            new SetProperty()
                            {
                                Property = "turn.processInput",
                                Value = "true"
                            }
                        }
                    },
                }
            };

            await CreateFlow(rootDialog)
            .Send("start")
                .AssertReply("What is your name?")
            .Send("interrupt")
                .AssertReply("In Interruption...")
                .AssertReply("I have interrupt as your name")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_ReProcessInputPropertyValidOnlyOnce()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("Interruption", "(?i)interrupt"),
                        new IntentPattern("Start", "(?i)start"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent()
                    {
                        Intent = "Start",
                        Actions = new List<Dialog>()
                        {
                            new TextInput()
                            {
                                Prompt = new ActivityTemplate("What is your name?"),
                                Property = "user.name",
                                AllowInterruptions = AllowInterruptions.Always
                            },
                            new SendActivity("I have {user.name} as your name"),
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("What is your age?"),
                                Property = "user.age",
                                AllowInterruptions = AllowInterruptions.Always
                            },
                            new SendActivity("I have {user.age} as your age")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "Interruption",
                        Actions = new List<Dialog>()
                        {
                            // short circuiting Interruption so consultation is terminated. 
                            new SendActivity("In Interruption..."),

                            // request the active input step to re-process user input. 
                            new SetProperty()
                            {
                                Property = "turn.processInput",
                                Value = "true"
                            }
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "None",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("You said {turn.activity.text}"),
                            new SetProperty()
                            {
                                Property = "turn.processInput",
                                Value = "true"
                            }
                        }
                    }
                }
            };

            await CreateFlow(rootDialog)
            .Send("start")
                .AssertReply("What is your name?")
            .Send("interrupt")
                .AssertReply("In Interruption...")
                .AssertReply("I have interrupt as your name")
                .AssertReply("What is your age?")
            .Send("36")
                .AssertReply("You said 36")
                .AssertReply("I have 36 as your age")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_AllowInterruptionAlwaysWithFailedValidation()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("Start", "(?i)start"),
                        new IntentPattern("None", "200"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent()
                    {
                        Intent = "Start",
                        Actions = new List<Dialog>()
                        {
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("What is your age?"),
                                Property = "user.age",
                                AllowInterruptions = AllowInterruptions.Always,
                                Validations = new List<string>()
                                {
                                    "int(this.value) >= 1",
                                    "int(this.value) <= 150"
                                },
                                InvalidPrompt = new ActivityTemplate("Sorry. {this.value} does not work. I'm looking for a value between 1-150. What is your age?")
                            },
                            new SendActivity("I have {user.age} as your age")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "None",
                        Actions = new List<Dialog>()
                        {
                            // short circuiting Interruption so consultation is terminated. 
                            new SendActivity("In None..."),

                            // request the active input step to re-process user input. 
                            new SetProperty()
                            {
                                Property = "turn.processInput",
                                Value = "true"
                            }
                        }
                    },
                }
            };

            await CreateFlow(rootDialog)
            .Send("start")
                .AssertReply("What is your age?")
            .Send("200")
                .AssertReply("In None...")
                .AssertReply("Sorry. 200 does not work. I'm looking for a value between 1-150. What is your age?")
            .Send("500")
                .AssertReply("In None...")
                .AssertReply("Sorry. 500 does not work. I'm looking for a value between 1-150. What is your age?")
            .Send("36")
                .AssertReply("In None...")
                .AssertReply("I have 36 as your age")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_AllowInterruptionNotRecognizedWithFailedValidation()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("Start", "(?i)start"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent()
                    {
                        Intent = "Start",
                        Actions = new List<Dialog>()
                        {
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("What is your age?"),
                                Property = "user.age",
                                AllowInterruptions = AllowInterruptions.NotRecognized,
                                Validations = new List<string>()
                                {
                                    "int(this.value) >= 1",
                                    "int(this.value) <= 150"
                                },
                                InvalidPrompt = new ActivityTemplate("Sorry. {this.value} does not work. I'm looking for a value between 1-150. What is your age?")
                            },
                            new SendActivity("I have {user.age} as your age")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "None",
                        Actions = new List<Dialog>()
                        {
                            // short circuiting Interruption so consultation is terminated. 
                            new SendActivity("In None..."),

                            // request the active input step to re-process user input. 
                            new SetProperty()
                            {
                                Property = "turn.processInput",
                                Value = "true"
                            }
                        }
                    },
                }
            };

            await CreateFlow(rootDialog)
            .Send("start")
                .AssertReply("What is your age?")
            .Send("200")
                .AssertReply("Sorry. 200 does not work. I'm looking for a value between 1-150. What is your age?")
            .Send("500")
                .AssertReply("Sorry. 500 does not work. I'm looking for a value between 1-150. What is your age?")
            .Send("36")
                .AssertReply("I have 36 as your age")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_AllowInterruptionAlwaysWithUnrecognizedInput()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("Start", "(?i)start"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent()
                    {
                        Intent = "Start",
                        Actions = new List<Dialog>()
                        {
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("What is your age?"),
                                Property = "user.age",
                                AllowInterruptions = AllowInterruptions.Always,
                                UnrecognizedPrompt = new ActivityTemplate("Sorry. I did not recognize a number. What is your age?")
                            },
                            new SendActivity("I have {user.age} as your age")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "None",
                        Actions = new List<Dialog>()
                        {
                            // short circuiting Interruption so consultation is terminated. 
                            new SendActivity("In None..."),

                            // request the active input step to re-process user input. 
                            new SetProperty()
                            {
                                Property = "turn.processInput",
                                Value = "true"
                            }
                        }
                    },
                }
            };

            await CreateFlow(rootDialog)
            .Send("start")
                .AssertReply("What is your age?")
            .Send("santa")
                .AssertReply("In None...")
                .AssertReply("Sorry. I did not recognize a number. What is your age?")
            .Send("red")
                .AssertReply("In None...")
                .AssertReply("Sorry. I did not recognize a number. What is your age?")
            .Send("36")
                .AssertReply("In None...")
                .AssertReply("I have 36 as your age")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_AllowInterruptionNotRecognizedWithUnrecognizedInput()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("Start", "(?i)start"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent()
                    {
                        Intent = "Start",
                        Actions = new List<Dialog>()
                        {
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("What is your age?"),
                                Property = "user.age",
                                AllowInterruptions = AllowInterruptions.NotRecognized,
                                UnrecognizedPrompt = new ActivityTemplate("Sorry. I did not recognize a number. What is your age?")
                            },
                            new SendActivity("I have {user.age} as your age")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "None",
                        Actions = new List<Dialog>()
                        {
                            // short circuiting Interruption so consultation is terminated. 
                            new SendActivity("In None..."),

                            // request the active input step to re-process user input. 
                            new SetProperty()
                            {
                                Property = "turn.processInput",
                                Value = "true"
                            }
                        }
                    },
                }
            };

            await CreateFlow(rootDialog)
            .Send("start")
                .AssertReply("What is your age?")
            .Send("santa")
                .AssertReply("In None...")
                .AssertReply("Sorry. I did not recognize a number. What is your age?")
            .Send("red")
                .AssertReply("In None...")
                .AssertReply("Sorry. I did not recognize a number. What is your age?")
            .Send("36")
                .AssertReply("I have 36 as your age")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_AllowInterruptionNever()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("Start", "(?i)start"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent()
                    {
                        Intent = "Start",
                        Actions = new List<Dialog>()
                        {
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("What is your age?"),
                                Property = "user.age",
                                AllowInterruptions = AllowInterruptions.Never,
                            },
                            new SendActivity("I have {user.age} as your age")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "None",
                        Actions = new List<Dialog>()
                        {
                            // short circuiting Interruption so consultation is terminated. 
                            new SendActivity("In None..."),

                            // request the active input step to re-process user input. 
                            new SetProperty()
                            {
                                Property = "turn.processInput",
                                Value = "true"
                            }
                        }
                    },
                }
            };

            await CreateFlow(rootDialog)
            .Send("start")
                .AssertReply("What is your age?")
            .Send("santa")
                .AssertReply("What is your age?")
            .Send("red")
                .AssertReply("What is your age?")
            .Send("36")
                .AssertReply("I have 36 as your age")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_AllowInterruptionNeverWithUnrecognizedInput()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("Start", "(?i)start"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent()
                    {
                        Intent = "Start",
                        Actions = new List<Dialog>()
                        {
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("What is your age?"),
                                Property = "user.age",
                                AllowInterruptions = AllowInterruptions.Never,
                                UnrecognizedPrompt = new ActivityTemplate("Sorry. I did not recognize a number. What is your age?")
                            },
                            new SendActivity("I have {user.age} as your age")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "None",
                        Actions = new List<Dialog>()
                        {
                            // short circuiting Interruption so consultation is terminated. 
                            new SendActivity("In None..."),

                            // request the active input step to re-process user input. 
                            new SetProperty()
                            {
                                Property = "turn.processInput",
                                Value = "true"
                            }
                        }
                    },
                }
            };

            await CreateFlow(rootDialog)
            .Send("start")
                .AssertReply("What is your age?")
            .Send("santa")
                .AssertReply("Sorry. I did not recognize a number. What is your age?")
            .Send("red")
                .AssertReply("Sorry. I did not recognize a number. What is your age?")
            .Send("36")
                .AssertReply("I have 36 as your age")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_AllowInterruptionNeverWithInvalidInput()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("Start", "(?i)start"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent()
                    {
                        Intent = "Start",
                        Actions = new List<Dialog>()
                        {
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("What is your age?"),
                                Property = "user.age",
                                AllowInterruptions = AllowInterruptions.Never,
                                Validations = new List<string>()
                                {
                                    "int(this.value) >= 1",
                                    "int(this.value) <= 150"
                                },
                                InvalidPrompt = new ActivityTemplate("Sorry. {this.value} does not work. I'm looking for a value between 1-150. What is your age?")
                            },
                            new SendActivity("I have {user.age} as your age")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "None",
                        Actions = new List<Dialog>()
                        {
                            // short circuiting Interruption so consultation is terminated. 
                            new SendActivity("In None..."),

                            // request the active input step to re-process user input. 
                            new SetProperty()
                            {
                                Property = "turn.processInput",
                                Value = "true"
                            }
                        }
                    },
                }
            };

            await CreateFlow(rootDialog)
            .Send("start")
                .AssertReply("What is your age?")
            .Send("200")
                .AssertReply("Sorry. 200 does not work. I'm looking for a value between 1-150. What is your age?")
            .Send("500")
                .AssertReply("Sorry. 500 does not work. I'm looking for a value between 1-150. What is your age?")
            .Send("36")
                .AssertReply("I have 36 as your age")
            .StartTestAsync();
        }

        [TestMethod]

        public async Task AdaptiveDialog_AllowInterruptionNeverWithMaxCount()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("Start", "(?i)start"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent()
                    {
                        Intent = "Start",
                        Actions = new List<Dialog>()
                        {
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("What is your age?"),
                                Property = "user.age",
                                AllowInterruptions = AllowInterruptions.Never,
                                MaxTurnCount = 2,
                                DefaultValue = "30"
                            },
                            new SendActivity("I have {user.age} as your age")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "None",
                        Actions = new List<Dialog>()
                        {
                            // short circuiting Interruption so consultation is terminated. 
                            new SendActivity("In None..."),

                            // request the active input step to re-process user input. 
                            new SetProperty()
                            {
                                Property = "turn.processInput",
                                Value = "true"
                            }
                        }
                    },
                }
            };

            await CreateFlow(rootDialog)
            .Send("start")
                .AssertReply("What is your age?")
            .Send("vishwac")
                .AssertReply("What is your age?")
            .Send("carlos")
                .AssertReply("I have 30 as your age")
            .StartTestAsync();
        }

        public async Task TestBindingTwoWayAcrossAdaptiveDialogs(object options)
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Triggers = new List<OnCondition>()
                {
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
                        {
                            new TextInput()
                            {
                                Property = "$name",
                                Prompt = new ActivityTemplate("Hello, what is your name?")
                            },
                            new BeginDialog("ageDialog")
                            {
                                Options = options,
                                ResultProperty = "$age"
                            },
                            new SendActivity("Hello {$name}, you are {$age} years old!")
                        }
                    }
                }
            };

            var ageDialog = new AdaptiveDialog("ageDialog")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
                        {
                            new NumberInput()
                            {
                                Prompt = new ActivityTemplate("Hello {$options.userName}, how old are you?"),
                                Property = "$age"
                            },
                            new EndDialog()
                            {
                                Value = "$age"
                            }
                        }
                    }
                }
            };

            rootDialog.Dialogs.Add(ageDialog);

            await CreateFlow(rootDialog)
            .Send("Hi")
                .AssertReply("Hello, what is your name?")
            .Send("zoidberg")
                .AssertReply("Hello zoidberg, how old are you?")
            .Send("I'm 77")
                .AssertReply("Hello zoidberg, you are 77 years old!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_AdaptiveCardSubmit()
        {
            var ruleDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("SubmitIntent", "123123123"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent(intent: "SubmitIntent")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("The city is {@city}!")
                        }
                    },
                },
            };

            var submitActivity = Activity.CreateMessageActivity();
            submitActivity.Value = new
            {
                intent = "SubmitIntent",
                city = "Seattle"
            };

            await CreateFlow(ruleDialog)
               .Send(submitActivity)
                   .AssertReply("The city is Seattle!")
               .StartTestAsync();
        }

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
                .UseAdaptiveDialogs()
                .UseLanguageGeneration(explorer)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            DialogManager dm = new DialogManager(ruleDialog);
            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Test class to test two way binding with strongly typed options objects.
        /// </summary>
        private class Person
        {
            public string UserName { get; set; }
        }
    }
}
