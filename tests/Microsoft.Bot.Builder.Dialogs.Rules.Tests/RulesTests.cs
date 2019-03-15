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
    public class RulesTests
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

        [TestMethod]
        public async Task Planning_IntentRule()
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
