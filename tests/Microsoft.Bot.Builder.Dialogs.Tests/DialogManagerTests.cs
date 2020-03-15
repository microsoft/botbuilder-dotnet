// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1402 // File may only contain a single type

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
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
            var firstConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "conversation.name");

            await CreateFlow(adaptiveDialog, storage, firstConversationId)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .Send("hi")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task DialogManager_AlternateProperty()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "conversation.name");

            await CreateFlow(adaptiveDialog, storage, firstConversationId, dialogStateProperty: "dialogState")
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
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

        [TestMethod]
        public async Task DialogManager_OnErrorEvent_Leaf()
        {
            await TestUtilities.RunTestScript();
        }

        [TestMethod]
        public async Task DialogManager_OnErrorEvent_Parent()
        {
            await TestUtilities.RunTestScript();
        }

        [TestMethod]
        public async Task DialogManager_OnErrorEvent_Root()
        {
            await TestUtilities.RunTestScript();
        }

        [TestMethod]
        public async Task DialogManager_DialogSet()
        {
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter();
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            var rootDialog = new AdaptiveDialog()
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty()
                            {
                                Property = "conversation.dialogId",
                                Value = "test"
                            },
                            new BeginDialog()
                            {
                                Dialog = "=conversation.dialogId"
                            },
                            new BeginDialog()
                            {
                                Dialog = "test"
                            }
                        }
                    }
                }
            };

            DialogManager dm = new DialogManager(rootDialog);
            dm.Dialogs.Add(new SimpleDialog() { Id = "test" });

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
                })
                .SendConversationUpdate()
                    .AssertReply("simple")
                    .AssertReply("simple")
                .StartTestAsync();
        }

        private Dialog CreateTestDialog(string property = "user.name")
        {
            return new AskForNameDialog(property.Replace(".", string.Empty), property);
        }

        private TestFlow CreateFlow(Dialog dialog, IStorage storage, string conversationId, string dialogStateProperty = null)
        {
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(conversationId));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            DialogManager dm = new DialogManager(dialog, dialogStateProperty: dialogStateProperty);
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

            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default)
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
                return this.Dialogs.GetDialogs();
            }

            public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
            {
                outerDc.State.SetValue(this.Property, result);
                await outerDc.Context.SendActivityAsync($"Hello {result.ToString()}, nice to meet you!");
                return await outerDc.EndDialogAsync(result);
            }
        }

        public class SimpleDialog : Dialog
        {
            public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                await dc.Context.SendActivityAsync("simple");
                return await dc.EndDialogAsync();
            }
        }
    }
}
