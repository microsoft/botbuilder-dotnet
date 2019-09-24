// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.LanguageGeneration.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class DialogManagerTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task DialogManager_ConversationState_PersistedAcrossTurns()
        {
            var conversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "conversation.name");

            await CreateFlow(adaptiveDialog, storage, conversationId)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();

            await CreateFlow(adaptiveDialog, storage, conversationId)
            .Send("hi")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task DialogManager_ConversationState_ClearedAcrossConversations()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var secondConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "conversation.name");

            await CreateFlow(adaptiveDialog, storage, firstConversationId)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();

            await CreateFlow(adaptiveDialog, storage, secondConversationId)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("John")
                .AssertReply("Hello John, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task DialogManager_UserState_PersistedAcrossConversations()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var secondConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "user.name");

            await CreateFlow(adaptiveDialog, storage, firstConversationId)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();

            await CreateFlow(adaptiveDialog, storage, secondConversationId)
            .Send("hi")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task DialogManager_UserState_NestedDialogs_PersistedAcrossConversations()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var secondConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var outerAdaptiveDialog = CreateTestDialog(property: "user.name");

            var componentDialog = new ComponentDialog();
            componentDialog.AddDialog(outerAdaptiveDialog);

            await CreateFlow(componentDialog, storage, firstConversationId)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();

            await CreateFlow(componentDialog, storage, secondConversationId)
            .Send("hi")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        private AdaptiveDialog CreateTestDialog(string property = "user.name")
        {
            var adaptiveDialog = new AdaptiveDialog("planningTest");

            adaptiveDialog.Triggers.Add(
                new OnUnknownIntent(
                    new List<Dialog>()
                    {
                        new TextInput()
                        {
                            Prompt = new ActivityTemplate("Hello, what is your name?"),
                            Property = property
                        },
                        new SendActivity($"Hello {{{property}}}, nice to meet you!"),
                    }));

            return adaptiveDialog;
        }

        private TestFlow CreateFlow(Dialog adaptiveDialog, IStorage storage, string conversationId)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();

            var explorer = new ResourceExplorer();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(conversationId));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .Use(new RegisterClassMiddleware<ResourceExplorer>(explorer))
                .UseAdaptiveDialogs()
                .UseLanguageGeneration(explorer)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            DialogManager dm = new DialogManager(adaptiveDialog);
            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }
    }
}
