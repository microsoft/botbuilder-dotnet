// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Expressions;
using Microsoft.Bot.Builder.Dialogs.Rules.Expressions;
using Microsoft.Bot.Builder.Dialogs.Rules.Input;
using Microsoft.Bot.Builder.Dialogs.Rules.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Rules.Rules;
using Microsoft.Bot.Builder.Dialogs.Rules.Steps;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Tests
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
                .Use(new RegisterClassMiddleware<IExpressionFactory>(new CommonExpressionFactory()))
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

            ruleDialog.AddRule(new NoMatchRule(
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

            ruleDialog.AddRule(new NoMatchRule(new List<IDialog>()
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
                new NoMatchRule(
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

            var planningDialog = new AdaptiveDialog("planningTest");

            planningDialog.AddRules(new List<IRule>()
            {
                new NoMatchRule(
                    new List<IDialog>()
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
                    })
            });

            await CreateFlow(planningDialog, convoState, userState)
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

            ruleDialog.AddRule(new NoMatchRule(
                    new List<IDialog>()
                    {
                        new IfCondition()
                        {
                            Condition = new CommonExpression("user.name == null"),
                            IfTrue = new List<IDialog>()
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

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(
                new NoMatchRule(
                    new List<IDialog>()
                    {
                        new IfCondition()
                        {
                            Condition = new CommonExpression("user.name == null"),
                            IfTrue = new List<IDialog>()
                            {
                                new TextInput()
                                {
                                    Prompt = new ActivityTemplate("Hello, what is your name?"),
                                    Property = "user.name"
                                }
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
        public async Task AdaptiveDialog_WelcomeRule()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRules(new List<IRule>()
            {
                new WelcomeRule(
                    new List<IDialog>()
                    {
                        new SendActivity("Welcome my friend!")
                    }),
                new NoMatchRule(
                    new List<IDialog>()
                    {
                        new IfCondition()
                        {
                            Condition = new CommonExpression("user.name == null"),
                            IfTrue = new List<IDialog>()
                            {
                                new TextInput()
                                {
                                    Prompt = new ActivityTemplate("Hello, what is your name?"),
                                    Property = "user.name"
                                }
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    })});

            await CreateFlow(ruleDialog, convoState, userState)
            .Send(new Activity() { Type = ActivityTypes.ConversationUpdate, MembersAdded = new List<ChannelAccount>() { new ChannelAccount("bot", "Bot"), new ChannelAccount("user", "User") } })
            .Send("hi")
                .AssertReply("Welcome my friend!")
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

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRules(new List<IRule>()
            {
                new NoMatchRule(
                    new List<IDialog>()
                    {
                        new IfCondition()
                        {
                            Condition = new CommonExpression("user.name == null"),
                            IfTrue = new List<IDialog>()
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
                            Condition = new CommonExpression("user.name == 'Carlos'"),
                            IfTrue = new List<IDialog>()
                            {
                                new SendActivity("Hello carlin")
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    })});

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

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            ruleDialog.AddRules(new List<IRule>()
            {
                new IntentRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new EndTurn(),
                        new SendActivity("To get to the other side")
                    }),
                new NoMatchRule(
                    new List<IDialog>()
                    {
                        new IfCondition()
                        {
                            Condition = new CommonExpression("user.name == null"),
                            IfTrue = new List<IDialog>()
                            {
                                new TextInput()
                                {
                                    Prompt = new ActivityTemplate("Hello, what is your name?"),
                                    Property = "user.name"
                                }
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    })});

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
            .StartTestAsync();
        }

        [TestMethod]
        public async Task AdaptiveDialog_ReplacePlan()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            ruleDialog.AddRules(new List<IRule>()
            {
                new IntentRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new EndTurn(),
                        new SendActivity("To get to the other side")
                    }),
                new WelcomeRule(
                    steps: new List<IDialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                    }),
                new NoMatchRule(
                    new List<IDialog>()
                    {
                        new IfCondition()
                        {
                            Condition = new CommonExpression("user.name == null"),
                            IfTrue = new List<IDialog>()
                            {
                                new TextInput()
                                {
                                    Prompt = new ActivityTemplate("Hello, what is your name?"),
                                    Property = "user.name"
                                }
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    })});

            await CreateFlow(ruleDialog, convoState, userState)
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
        public async Task AdaptiveDialog_NestedInlineSequences()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            ruleDialog.AddRules(new List<IRule>()
            {
                new IntentRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new AdaptiveDialog("TellJokeDialog")
                        {
                            Rules = new List<IRule>() {
                                new NoMatchRule(new List<IDialog>()
                                {
                                    new SendActivity("Why did the chicken cross the road?"),
                                    new EndTurn(),
                                    new SendActivity("To get to the other side")
                                })
                            }
                        }
                    }),
                new WelcomeRule(
                    steps: new List<IDialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                    }),
                new NoMatchRule(
                    new List<IDialog>()
                    {
                        new AdaptiveDialog("AskNameDialog")
                        {
                            Rules = new List<IRule>()
                            {
                                new NoMatchRule(new List<IDialog>()
                                    {
                                        new IfCondition()
                                        {
                                            Condition = new CommonExpression("user.name == null"),
                                            IfTrue = new List<IDialog>()
                                            {
                                                new TextInput()
                                                {
                                                    Prompt = new ActivityTemplate("Hello, what is your name?"),
                                                    Property = "user.name"
                                                }
                                            }
                                        },
                                        new SendActivity("Hello {user.name}, nice to meet you!")
                                    })
                            }
                        }
                    }
                )
            });

            await CreateFlow(ruleDialog, convoState, userState)
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
        public async Task AdaptiveDialog_BeginDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            ruleDialog.AddRules(new List<IRule>()
            {
                new IntentRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new BeginDialog("TellJokeDialog")
                    }),
                new WelcomeRule(
                    steps: new List<IDialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                    }),
                new NoMatchRule(
                    new List<IDialog>()
                    {
                        new BeginDialog("AskNameDialog")
                    })});

            ruleDialog.AddDialog(new[] {
                new AdaptiveDialog("AskNameDialog")
                {
                    Rules = new List<IRule>()
                    {
                        new NoMatchRule(new List<IDialog>()
                        {
                            new IfCondition()
                            {
                                Condition = new CommonExpression("user.name == null"),
                                IfTrue = new List<IDialog>()
                                {
                                    new TextInput()
                                    {
                                        Prompt = new ActivityTemplate("Hello, what is your name?"),
                                        Property = "user.name"
                                    }
                                }
                            },
                            new SendActivity("Hello {user.name}, nice to meet you!")
                        })
                    }
                }

                });

            ruleDialog.AddDialog(new[] {
                new AdaptiveDialog("TellJokeDialog")
                    {
                        Rules = new List<IRule>() {
                            new NoMatchRule(new List<IDialog>()
                            {
                                new SendActivity("Why did the chicken cross the road?"),
                                new EndTurn(),
                                new SendActivity("To get to the other side")
                            })
                        }
                    }
                });

            await CreateFlow(ruleDialog, convoState, userState)
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
        public async Task AdaptiveDialog_IntentRule()
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
                new WelcomeRule(
                    steps: new List<IDialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                    })});

            await CreateFlow(planningDialog, convoState, userState)
            .Send("Do you know a joke?")
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
                .AssertReply("Why did the chicken cross the road?")
            .Send("Why?")
                .AssertReply("To get to the other side")
            .StartTestAsync();
        }
    }
}
