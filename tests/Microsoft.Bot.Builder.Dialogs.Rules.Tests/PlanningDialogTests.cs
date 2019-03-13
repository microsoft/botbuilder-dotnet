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
using Microsoft.Bot.Builder.Dialogs.Declarative.Expressions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Rules.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Rules.Rules;
using Microsoft.Bot.Builder.Dialogs.Rules.Steps;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Tests
{
    [TestClass]
    public class PlanningDialogTests
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
        public async Task Planning_TopLevelFallback()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new RuleDialog("planningTest");

            planningDialog.AddRule(new List<IRule>()
            {
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new SendActivity("Hello Planning!")
                    })});

            await CreateFlow(planningDialog, convoState, userState)
            .Send("start")
                .AssertReply("Hello Planning!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Planning_TopLevelFallbackMultipleActivities()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new RuleDialog("planningTest");

            planningDialog.AddRule(new List<IRule>()
            {
                new FallbackRule(
                    new List<IDialog>()
                    {
                        new SendActivity("Hello Planning!"),
                        new SendActivity("Howdy awain")
                    })});

            await CreateFlow(planningDialog, convoState, userState)
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
        public async Task Planning_ListManage()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new RuleDialog("planningTest");

            planningDialog.AddRule(new List<IRule>()
            {
                new FallbackRule(
                    new List<IDialog>()
                    {
                        // Add item
                        new SendActivity("Please add an item to todos."),
                        new WaitForInput("user.todo"),
                        new ChangeList(ChangeList.ChangeListType.push, "user.todos", "user.todo"),
                        new SendList("user.todos"),
                        new SendActivity("Please add an item to todos."),
                        new WaitForInput("user.todo"),
                        new ChangeList(ChangeList.ChangeListType.push, "user.todos", "user.todo"),
                        new SendList("user.todos"),

                        // Remove item
                        new SendActivity("Enter a item to remove."),
                        new WaitForInput("user.todo"),
                        new ChangeList(ChangeList.ChangeListType.remove, "user.todos", "user.todo"),
                        new SendList("user.todos"),

                        // Add item and pop item
                        new SendActivity("Please add an item to todos."),
                        new WaitForInput("user.todo"),
                        new ChangeList(ChangeList.ChangeListType.push, "user.todos", "user.todo"),
                        new SendActivity("Please add an item to todos."),
                        new WaitForInput("user.todo"),
                        new ChangeList(ChangeList.ChangeListType.push, "user.todos", "user.todo"),
                        new SendList("user.todos"),
                        new ChangeList(ChangeList.ChangeListType.pop, "user.todos"),
                        new SendList("user.todos"),

                        // Take item
                        new ChangeList(ChangeList.ChangeListType.take, "user.todos"),
                        new SendList("user.todos"),

                        // Clear list
                        new ChangeList(ChangeList.ChangeListType.clear, "user.todos"),
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
        public async Task Planning_WelcomeRule()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new RuleDialog("planningTest");
            
            planningDialog.AddRule(new List<IRule>()
            {
                new WelcomeRule(
                    new List<IDialog>()
                    {
                        new SendActivity("Welcome my friend!")
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
            .Send(new Activity() { Type = ActivityTypes.ConversationUpdate, MembersAdded = new List<ChannelAccount>() { new ChannelAccount("bot", "Bot") } })
            .Send("hi")
                .AssertReply("Welcome my friend!")
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

            planningDialog.Recognizer = new RegexRecognizer() { Rules = new Dictionary<string, string>() { { "JokeIntent", "joke" }  } };

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
        public async Task Planning_ReplacePlan()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var planningDialog = new RuleDialog("planningTest");

            planningDialog.Recognizer = new RegexRecognizer() { Rules = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            planningDialog.AddRule(new List<IRule>()
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

        //[TestMethod]
        //public async Task Planning_NestedInlineSequences()
        //{
        //    var convoState = new ConversationState(new MemoryStorage());
        //    var userState = new UserState(new MemoryStorage());

        //    var planningDialog = new RuleDialog("planningTest");

        //    planningDialog.Recognizer = new RegexRecognizer() { Rules = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

        //    planningDialog.AddRule(new List<IRule>()
        //    {
        //        new ReplacePlanRule("JokeIntent",
        //            steps: new List<IDialog>()
        //            {
        //                new SequenceDialog("TellJokeDialog",
        //                    new List<IDialog>()
        //                    {
        //                        new SendActivity("Why did the chicken cross the road?"),
        //                        new WaitForInput(),
        //                        new SendActivity("To get to the other side")
        //                    })
        //            }),
        //        new WelcomeRule(
        //            steps: new List<IDialog>()
        //            {
        //                new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
        //            }),
        //        new FallbackRule(
        //            new List<IDialog>()
        //            {
        //                new SequenceDialog("AskNameDialog",
        //                    new List<IDialog>()
        //                    {
        //                        new IfProperty()
        //                        {
        //                            Expression = new CommonExpression("user.name == null"),
        //                            IfTrue = new List<IDialog>()
        //                            {
        //                                new TextPrompt()
        //                                {
        //                                    InitialPrompt = new ActivityTemplate("Hello, what is your name?"),
        //                                    OutputBinding = "user.name"
        //                                }
        //                            }
        //                        },
        //                        new SendActivity("Hello {user.name}, nice to meet you!")
        //                    })
        //            })});

        //    await CreateFlow(planningDialog, convoState, userState)
        //    .Send("hi")
        //        .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
        //        .AssertReply("Hello, what is your name?")
        //    .Send("Carlos")
        //        .AssertReply("Hello Carlos, nice to meet you!")
        //    .Send("Do you know a joke?")
        //        .AssertReply("Why did the chicken cross the road?")
        //    .Send("Why?")
        //        .AssertReply("To get to the other side")
        //    .Send("hi")
        //        .AssertReply("Hello Carlos, nice to meet you!")
        //    .StartTestAsync();
        //}

        //[TestMethod]
        //public async Task Planning_CallDialog()
        //{
        //    var convoState = new ConversationState(new MemoryStorage());
        //    var userState = new UserState(new MemoryStorage());

        //    var planningDialog = new RuleDialog("planningTest");

        //    planningDialog.Recognizer = new RegexRecognizer() { Rules = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

        //    planningDialog.AddRule(new List<IRule>()
        //    {
        //        new ReplacePlanRule("JokeIntent",
        //            steps: new List<IDialog>()
        //            {
        //                new CallDialog("TellJokeDialog")
        //            }),
        //        new WelcomeRule(
        //            steps: new List<IDialog>()
        //            {
        //                new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
        //            }),
        //        new FallbackRule(
        //            new List<IDialog>()
        //            {
        //                new CallDialog("AskNameDialog")
        //            })});

        //    planningDialog.AddDialog(new[] {
        //        new SequenceDialog("AskNameDialog",
        //            new List<IDialog>()
        //            {
        //                new IfProperty()
        //                {
        //                    Expression = new CommonExpression("user.name == null"),
        //                    IfTrue = new List<IDialog>()
        //                    {
        //                        new TextPrompt()
        //                        {
        //                            InitialPrompt = new ActivityTemplate("Hello, what is your name?"),
        //                            OutputBinding = "user.name"
        //                        }
        //                    }
        //                },
        //                new SendActivity("Hello {user.name}, nice to meet you!")
        //            })
        //        });

        //    planningDialog.AddDialog(new[] {
        //        new SequenceDialog("TellJokeDialog",
        //            new List<IDialog>()
        //            {
        //                new SendActivity("Why did the chicken cross the road?"),
        //                new WaitForInput(),
        //                new SendActivity("To get to the other side")
        //            })
        //        });

        //    await CreateFlow(planningDialog, convoState, userState)
        //    .Send("hi")
        //        .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
        //        .AssertReply("Hello, what is your name?")
        //    .Send("Carlos")
        //        .AssertReply("Hello Carlos, nice to meet you!")
        //    .Send("Do you know a joke?")
        //        .AssertReply("Why did the chicken cross the road?")
        //    .Send("Why?")
        //        .AssertReply("To get to the other side")
        //    .Send("hi")
        //        .AssertReply("Hello Carlos, nice to meet you!")
        //    .StartTestAsync();
        //}
    }
}
