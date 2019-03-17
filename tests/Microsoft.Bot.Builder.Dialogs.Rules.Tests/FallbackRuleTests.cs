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
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Rules.Rules;
using Microsoft.Bot.Builder.Dialogs.Rules.Steps;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Tests
{
    [TestClass]
    public class FallbackRuleTests
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

            var userStateProperty = userState.CreateProperty<Dictionary<string, object>>("user");
            var convoStateProperty = convoState.CreateProperty<Dictionary<string, object>>("conversation");

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);

            planningDialog.Storage = new MemoryStorage();

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await planningDialog.OnTurnAsync(turnContext, null).ConfigureAwait(false);
            });
        }

        [TestMethod]
        public async Task FallbackRule_TopLevelFallback()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new RuleDialog("sequenceTest");
            ruleDialog.AddRule(new FallbackRule(
                new List<IDialog>()
                {
                    new SendActivity("Hello Planning!")
                }));

            ruleDialog.UserState = userState.CreateProperty<StateMap>("userStateProperty");
            ruleDialog.BotState = convoState.CreateProperty<BotState>("botStateProperty");

            await CreateFlow(ruleDialog, convoState, userState)
            .Send("start")
                .AssertReply("Hello Planning!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task FallbackRule_TopLevelFallbackMultipleActivities()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new RuleDialog("planningTest");
            ruleDialog.AddRule(new FallbackRule(
                new List<IDialog>()
                    {
                        new SendActivity("Hello Planning!"),
                        new SendActivity("Howdy awain")
                    }));

            ruleDialog.UserState = userState.CreateProperty<StateMap>("userStateProperty");
            ruleDialog.BotState = convoState.CreateProperty<BotState>("botStateProperty");


            await CreateFlow(ruleDialog, convoState, userState)
            .Send("start")
                .AssertReply("Hello Planning!")
                .AssertReply("Howdy awain")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task FallbackRule_WaitForInput()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new RuleDialog("planningTest");
            ruleDialog.AddRule(new FallbackRule(new List<IDialog>()
                    {
                        new SendActivity("Hello, what is your name?"),
                        new WaitForInput("user.name"),
                        new SendActivity("Hello {user.name}, nice to meet you!"),
                    }));

            ruleDialog.UserState = userState.CreateProperty<StateMap>("userStateProperty");
            ruleDialog.BotState = convoState.CreateProperty<BotState>("botStateProperty");

            await CreateFlow(ruleDialog, convoState, userState)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }
    }
}
