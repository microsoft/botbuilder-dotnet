// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
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

        private Dialog CreateTestDialog(string property = "user.name")
        {
            return new AskForNameDialog(property.Replace(".", string.Empty), property);
        }

        private TestFlow CreateFlow(Dialog dialog, IStorage storage, string conversationId)
        {
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(conversationId));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            DialogManager dm = new DialogManager(dialog);
            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }

        public class AskForNameDialog : ComponentDialog, IDialogDependencies
        {
            public AskForNameDialog(string id, string property)
                : base(id)
            {
                AddDialog(new TextPrompt("prompt"));
                this.Property = property;
            }

            public string Property { get; set; }

            public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default)
            {
                if (outerDc.State.TryGetValue<string>(this.Property, out string result))
                {
                    await outerDc.Context.SendActivityAsync($"Hello {result.ToString()}, nice to meet you!");
                    return await outerDc.EndDialogAsync(result);
                }

                return await outerDc.BeginDialogAsync(
                    "prompt",
                    new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Hello, what is your name?" },
                        RetryPrompt = new Activity { Type = ActivityTypes.Message, Text = "Hello, what is your name?" },
                    },
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            public IEnumerable<Dialog> GetDependencies()
            {
                return this._dialogs.GetDialogs();
            }

            public async override Task<DialogTurnResult> ResumeDialogAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
            {
                outerDc.State.SetValue(this.Property, result);
                await outerDc.Context.SendActivityAsync($"Hello {result.ToString()}, nice to meet you!");
                return await outerDc.EndDialogAsync(result);
            }
        }
    }
}
