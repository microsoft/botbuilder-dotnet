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
        public async Task Planning_TopLevelFallback()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(new DefaultRule(
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
        public async Task Planning_TopLevelFallbackMultipleActivities()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(new DefaultRule(new List<IDialog>()
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
        public async Task Planning_WaitForInput()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(
                new DefaultRule(
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
        public async Task Planning_ListManage()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest");

            planningDialog.AddRules(new List<IRule>()
            {
                new DefaultRule(
                    new List<IDialog>()
                    {
                        // Add item
                        new TextInput() {
                            Prompt = new ActivityTemplate("Please add an item to todos."),
                            Property = "dialog.todo"
                        },
                        new ChangeList(ChangeList.ChangeListType.Push, "user.todos", "dialog.todo"),
                        new SendList("user.todos"),
                        new TextInput()
                        {
                            Prompt = new ActivityTemplate("Please add an item to todos."),
                            Property = "dialog.todo"
                        },
                        new ChangeList(ChangeList.ChangeListType.Push, "user.todos", "dialog.todo"),
                        new SendList("user.todos"),

                        // Remove item
                        new TextInput() {
                            Prompt = new ActivityTemplate("Enter a item to remove."),
                            Property = "dialog.todo"
                        },
                        new ChangeList(ChangeList.ChangeListType.Remove, "user.todos", "dialog.todo"),
                        new SendList("user.todos"),

                        // Add item and pop item
                        new TextInput() {
                            Prompt = new ActivityTemplate("Please add an item to todos."),
                            Property = "dialog.todo"
                        },
                        new ChangeList(ChangeList.ChangeListType.Push, "user.todos", "dialog.todo"),
                        new TextInput()
                        {
                            Prompt = new ActivityTemplate("Please add an item to todos."),
                            Property = "dialog.todo"
                        },
                        new ChangeList(ChangeList.ChangeListType.Push, "user.todos", "dialog.todo"),
                        new SendList("user.todos"),

                        new ChangeList(ChangeList.ChangeListType.Pop, "user.todos"),
                        new SendList("user.todos"),

                        // Take item
                        new ChangeList(ChangeList.ChangeListType.Take, "user.todos"),
                        new SendList("user.todos"),

                        // Clear list
                        new ChangeList(ChangeList.ChangeListType.Clear, "user.todos"),
                        new SendList("user.todos")
                    })
            });

            await CreateFlow(planningDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Please add an item to todos.")
            .Send("todo1")
                .AssertReply("- todo1\n")
                .AssertReply("Please add an item to todos.")
            .Send("todo2")
                .AssertReply("- todo1\n- todo2\n")
                .AssertReply("Enter a item to remove.")
            .Send("todo2")
                .AssertReply("- todo1\n")
                .AssertReply("Please add an item to todos.")
            .Send("todo3")
                .AssertReply("Please add an item to todos.")
            .Send("todo4")
                .AssertReply("- todo1\n- todo3\n- todo4\n")
                .AssertReply("- todo1\n- todo3\n")
                .AssertReply("- todo3\n")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Planning_IfProperty()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(new DefaultRule(
                    new List<IDialog>()
                    {
                        new IfProperty()
                        {
                            Expression = new CommonExpression("user.name == null"),
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
        public async Task Planning_TextPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(
                new DefaultRule(
                    new List<IDialog>()
                    {
                        new IfProperty()
                        {
                            Expression = new CommonExpression("user.name == null"),
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
        public async Task Planning_WelcomeRule()
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
                new DefaultRule(
                    new List<IDialog>()
                    {
                        new IfProperty()
                        {
                            Expression = new CommonExpression("user.name == null"),
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
        public async Task Planning_StringLiteralInExpression()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRules(new List<IRule>()
            {
                new DefaultRule(
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
                        new IfProperty()
                        {
                            // Check comparison with string literal
                            Expression = new CommonExpression("user.name == 'Carlos'"),
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
        public async Task Planning_DoSteps()
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
                        new WaitForInput(),
                        new SendActivity("To get to the other side")
                    }),
                new DefaultRule(
                    new List<IDialog>()
                    {
                        new IfProperty()
                        {
                            Expression = new CommonExpression("user.name == null"),
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
        public async Task Planning_ReplacePlan()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            ruleDialog.AddRules(new List<IRule>()
            {
                new ReplacePlanRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new WaitForInput(),
                        new SendActivity("To get to the other side")
                    }),
                new WelcomeRule(
                    steps: new List<IDialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                    }),
                new DefaultRule(
                    new List<IDialog>()
                    {
                        new IfProperty()
                        {
                            Expression = new CommonExpression("user.name == null"),
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
        public async Task Planning_NestedInlineSequences()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            ruleDialog.AddRules(new List<IRule>()
            {
                new ReplacePlanRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new AdaptiveDialog("TellJokeDialog")
                        {
                            Rules = new List<IRule>() {
                                new DefaultRule(new List<IDialog>()
                                {
                                    new SendActivity("Why did the chicken cross the road?"),
                                    new WaitForInput(),
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
                new DefaultRule(
                    new List<IDialog>()
                    {
                        new AdaptiveDialog("AskNameDialog")
                        {
                            Rules = new List<IRule>()
                            {
                                new DefaultRule(new List<IDialog>()
                                    {
                                        new IfProperty()
                                        {
                                            Expression = new CommonExpression("user.name == null"),
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
        public async Task Planning_CallDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            ruleDialog.AddRules(new List<IRule>()
            {
                new ReplacePlanRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new CallDialog("TellJokeDialog")
                    }),
                new WelcomeRule(
                    steps: new List<IDialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                    }),
                new DefaultRule(
                    new List<IDialog>()
                    {
                        new CallDialog("AskNameDialog")
                    })});

            ruleDialog.AddDialog(new[] {
                new AdaptiveDialog("AskNameDialog")
                {
                    Rules = new List<IRule>()
                    {
                        new DefaultRule(new List<IDialog>()
                        {
                            new IfProperty()
                            {
                                Expression = new CommonExpression("user.name == null"),
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
                            new DefaultRule(new List<IDialog>()
                            {
                                new SendActivity("Why did the chicken cross the road?"),
                                new WaitForInput(),
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
        public async Task Planning_IntentRule()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new AdaptiveDialog("planningTest");

            planningDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            planningDialog.AddRules(new List<IRule>()
            {
                new ReplacePlanRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new SendActivity("Why did the chicken cross the road?"),
                        new WaitForInput(),
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
