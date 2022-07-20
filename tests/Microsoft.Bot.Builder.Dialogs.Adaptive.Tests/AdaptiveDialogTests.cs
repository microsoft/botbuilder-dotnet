// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1210 // Using directives should be ordered alphabetically by namespace
#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1201 // Elements should appear in the correct order

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Xunit;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Schema;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Moq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [CollectionDefinition("Dialogs.Adaptive")]
    public class AdaptiveDialogTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public AdaptiveDialogTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(AdaptiveDialogTests));
        }

        [Fact]
        public async Task AdaptiveDialog_ActivityEvents()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_ActivityAndIntentEvents()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_AdaptiveCardSubmit()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_AllowInterruption()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_AllowInterruptionAlwaysWithFailedValidation()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_AllowInterruptionNever()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_AllowInterruptionNeverWithInvalidInput()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_AllowInterruptionNeverWithMaxCount()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_AllowInterruptionNeverWithUnrecognizedInput()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_AllowInterruptionWithMaxCount()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_LoadDialogFromProperty()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_BeginDialog()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_BeginDialog_With_Dup_Dialog_Ref()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_BindingCaptureValueWithinSameAdaptive()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_BindingOptionsAcrossAdaptiveDialogs()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_ForEachElement_BeginDialog()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_BindingReferValueInLaterAction()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_BindingReferValueInNestedAction()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_ConditionallyAllowInterruptions()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_DoActions()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_EditArray()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_EndTurn()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_IfProperty()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_NestedInlineSequences()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_NestedRecognizers()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_PropertySetInInterruption()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_ReplacePlan()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_ReProcessInputProperty()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_ReProcessInputPropertyValidOnlyOnce()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_StringLiteralInExpression()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_TextInput()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_TextInputDefaultValueResponse()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_TextInputNoMaxTurnCount()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_TopLevelFallback()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_TopLevelFallbackMultipleActivities()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_ParentBotInterruption()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestBindingTwoWayAcrossAdaptiveDialogs()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestForeachNullItems()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestForeachWithPrompt()
        {
               await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestForeachWithEndDialog()
        {
               await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestForeachWithPromptCachedItems()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestNestedForeachWithPromptCachedItems()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestBindingTwoWayAcrossAdaptiveDialogsDefaultResultProperty()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_EmitEventActivityReceived()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task AdaptiveDialog_NestedMemoryAccess()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestForEachElementReprompt()
        {
            var testFlow = new TestScript()
            {
                Dialog = new ForEachElementRepromptMainDialog()
            }
            .SendConversationUpdate();

            testFlow = testFlow.AssertReply("send me some text inside the old foreach");
            testFlow.Script.Add(new UserSays() { Text = "interrupt" });
            testFlow = testFlow.AssertReply("Hello from InterruptDialog");
            testFlow = testFlow.AssertReply("send me some text inside the old foreach");
            testFlow.Script.Add(new UserSays() { Text = "message one" });
            testFlow = testFlow.AssertReply("Foreach You said: 'message one'");

            testFlow = testFlow.AssertReply("send me some text inside the new foreach element");
            testFlow.Script.Add(new UserSays() { Text = "interrupt" });
            testFlow = testFlow.AssertReply("Hello from InterruptDialog");
            testFlow = testFlow.AssertReply("send me some text inside the new foreach element");
            testFlow.Script.Add(new UserSays() { Text = "message two" });
            testFlow = testFlow.AssertReply("ForEachElement You said: 'message two'");

            await testFlow.ExecuteAsync(_resourceExplorerFixture.ResourceExplorer);
        }
        
        [Fact]
        public async Task TestForEachElement_TelemetryTrackEvent()
        {
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();

            var testAdapter = new TestAdapter()
                .UseStorage(new MemoryStorage())
                .UseBotState(new ConversationState(new MemoryStorage()), new UserState(new MemoryStorage()));

            var rootDialog = new AdaptiveDialog
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new CodeAction(async (dc, obj) =>
                            {
                                dc.State.SetValue("$items", new List<string> { "1" });
                                return await dc.EndDialogAsync();
                            }),
                            new ForEachElement
                            {
                                ItemsProperty = "$items",
                                Actions = new List<Dialog>
                                {
                                    new TelemetryTrackEventAction("testEvent")
                                    {
                                        Properties = new Dictionary<string, StringExpression>()
                                        {
                                            { "prop1", "value1" },
                                            { "prop2", "value2" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                TelemetryClient = mockTelemetryClient.Object
            };

            var dialogManager = new DialogManager(rootDialog);

            await new TestFlow((TestAdapter)testAdapter, async (turnContext, cancellationToken) =>
            {
                await dialogManager.OnTurnAsync(turnContext, cancellationToken);
            })
            .SendConversationUpdate()
            .StartTestAsync();

            var trackEvent = mockTelemetryClient.Invocations.FirstOrDefault(i => i.Arguments[0]?.ToString() == "testEvent");

            Assert.NotNull(trackEvent);
            Assert.Equal(2, ((Dictionary<string, string>)trackEvent.Arguments[1]).Count);
            Assert.Equal("value1", ((Dictionary<string, string>)trackEvent.Arguments[1])["prop1"]);
            Assert.Equal("value2", ((Dictionary<string, string>)trackEvent.Arguments[1])["prop2"]);
        }

        private class ForEachElementRepromptMainDialog : ComponentDialog
        {
            public ForEachElementRepromptMainDialog()
                : base(nameof(ForEachElementRepromptMainDialog))
            {
                AddDialog(new FlowDialog());
                AddDialog(new InterruptDialog());
            }

            protected override Task<DialogTurnResult> OnBeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default)
                 => ProcessTurnAsync(dc, cancellationToken);

            protected override Task<DialogTurnResult> OnContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
                => ProcessTurnAsync(dc, cancellationToken);

            public async Task<DialogTurnResult> ProcessTurnAsync(DialogContext dc, CancellationToken cancellationToken = default)
            {
                if (dc.Context.Activity.Text == "interrupt")
                {
                    dc.Context.Activity.Text = string.Empty;
                    dc.Context.Activity.Value = null;
                    var interuptDialogTurnResult = await dc.BeginDialogAsync(nameof(InterruptDialog), null, cancellationToken).ConfigureAwait(false);

                    if (interuptDialogTurnResult.Status == DialogTurnStatus.CompleteAndWait || interuptDialogTurnResult.Status == DialogTurnStatus.Waiting)
                    {
                        return interuptDialogTurnResult;
                    }
                }

                if (dc.ActiveDialog == null)
                {
                    if (dc.Context.Activity.Type == ActivityTypes.ConversationUpdate)
                    {
                        var dialogTurnResult = await dc.BeginDialogAsync(nameof(FlowDialog)).ConfigureAwait(false);

                        switch (dialogTurnResult.Status)
                        {
                            case DialogTurnStatus.Empty:
                            case DialogTurnStatus.Complete:
                            case DialogTurnStatus.Cancelled:
                                dc.Context.Activity.Text = string.Empty;
                                dc.Context.Activity.Value = null;
                                return await dc.BeginDialogAsync(nameof(FlowDialog), null, cancellationToken).ConfigureAwait(false);
                            default:
                                return dialogTurnResult;
                        }
                    }
                    else
                    {
                        return await dc.BeginDialogAsync(nameof(FlowDialog));
                    }
                }
                else
                {
                    var dialogTurnResult = await dc.ContinueDialogAsync(cancellationToken);
                    switch (dialogTurnResult.Status)
                    {
                        case DialogTurnStatus.Empty:
                        case DialogTurnStatus.Complete:
                        case DialogTurnStatus.Cancelled:
                            dc.Context.Activity.Text = string.Empty;
                            dc.Context.Activity.Value = null;
                            dialogTurnResult = await dc.BeginDialogAsync(nameof(FlowDialog), null, cancellationToken).ConfigureAwait(false);
                            break;
                    }

                    return dialogTurnResult;
                }
            }

            private class InterruptDialog : ComponentDialog
            {
                public InterruptDialog()
                    : base(nameof(InterruptDialog))
                {
                    var proactiveDialog = new AdaptiveDialog(nameof(InterruptDialog)) { Generator = new TemplateEngineLanguageGenerator() };

                    proactiveDialog.Triggers.Add(new OnBeginDialog()
                    {
                        Actions =
                {
                    new CodeAction(async (dc, obj) =>
                    {
                        await dc.Context.SendActivityAsync($"Hello from {nameof(InterruptDialog)}");
                        return await dc.EndDialogAsync();
                    }),
                }
                    });

                    AddDialog(proactiveDialog);
                    InitialDialogId = nameof(InterruptDialog);
                }
            }

            private class FlowDialog : ComponentDialog
            {
                public FlowDialog()
                    : base(nameof(FlowDialog))
                {
                    var flowDialogStart = new AdaptiveDialog(nameof(FlowDialog)) { Generator = new TemplateEngineLanguageGenerator() };

                    flowDialogStart.Triggers.Add(new OnBeginDialog()
                    {
                        Actions =
                        {
                            new CodeAction(async (dc, obj) =>
                            {
                                dc.State.SetValue("$items", new List<string> { "1" });
                                return await dc.EndDialogAsync();
                            }),
                            new Foreach
                            {
                                ItemsProperty = "$items",
                                Actions = new List<Dialog>
                                {
                                    new InputDialogWithRePrompt<TextInput>(
                                            new TextInput
                                            {
                                                Prompt = new ActivityTemplate("send me some text inside the old foreach"),
                                                Property = "$answer",
                                                AlwaysPrompt = true
                                            },
                                            "inputtext"),
                                    new SendActivity { Activity = new ActivityTemplate("Foreach You said: '${$answer}'") }
                                }
                            },
                            new ForEachElement
                            {
                                ItemsProperty = "$items",
                                Actions = new List<Dialog>
                                {
                                    new InputDialogWithRePrompt<TextInput>(
                                            new TextInput
                                            {
                                                Prompt = new ActivityTemplate("send me some text inside the new foreach element"),
                                                Property = "$answer",
                                                AlwaysPrompt = true
                                            },
                                            "inputtext"),
                                    new SendActivity { Activity = new ActivityTemplate("ForEachElement You said: '${$answer}'") }
                                }
                            }
                        }
                    });

                    AddDialog(flowDialogStart);
                    InitialDialogId = nameof(FlowDialog);
                }
            }

            private class InputDialogWithRePrompt<T>
                : Dialog
                where T : InputDialog
            {
                public InputDialogWithRePrompt(T inputDialog, string id)
                {
                    InputDialog = inputDialog;
                    this.Id = id;
                }

                public T InputDialog { get; }

                public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
                {
                    this.DialogContext = dc;
                    return this.InputDialog.BeginDialogAsync(dc, options, cancellationToken);
                }

                public override Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
                {
                    this.DialogContext = dc;
                    return this.InputDialog.ContinueDialogAsync(dc, cancellationToken);
                }

                public override Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default)
                {
                    return this.InputDialog.EndDialogAsync(turnContext, instance, reason, cancellationToken);
                }

                public override Task<bool> OnDialogEventAsync(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
                {
                    this.DialogContext = dc;
                    return InputDialog.OnDialogEventAsync(dc, e, cancellationToken);
                }

                public override async Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default)
                {
                    var dialogContext = this.DialogContext ?? turnContext.TurnState.Get<DialogContext>();
                    if (dialogContext != null)
                    {
                        var messageActivity = await this.InputDialog.Prompt.BindAsync(dialogContext, null, cancellationToken);
                        await turnContext.SendActivityAsync(messageActivity, cancellationToken);
                    }
                }

                private DialogContext DialogContext { get; set; }
            }
        }

        [Theory]

        //[InlineData(1000)]
        [InlineData(500)]
        [InlineData(1)]
        public async Task TestForeachWithLargeItems(int itemCount)
        {
            var testFlow = new TestScript()
            {
                Dialog = new ForeachItemsDialog(itemCount)
            }
            .SendConversationUpdate();

            for (var i = 0; i < itemCount; i++)
            {
                testFlow = testFlow.AssertReply("Send me some text.");
                testFlow.Script.Add(new UserSays() { Text = "1" });
                testFlow = testFlow.AssertReply(i.ToString());
            }

            await testFlow.ExecuteAsync(_resourceExplorerFixture.ResourceExplorer);
        }

        private class ForeachItemsDialog : ComponentDialog
        {
            private readonly int _itemCount;

            internal ForeachItemsDialog(int itemCount)
            {
                _itemCount = itemCount;
                AddDialog(new AdaptiveDialog
                {
                    Id = "doItems",
                    Triggers = new List<OnCondition>
                    {
                        new OnBeginDialog
                        {
                            Actions = new List<Dialog>
                            {
                                new ForEachElement
                                {
                                    ItemsProperty = "$items",
                                    Actions = new List<Dialog>
                                    {
                                        new TextInput { Prompt = new ActivityTemplate("Send me some text.") },
                                        new SendActivity { Activity = new ActivityTemplate("${$foreach.value}") }
                                    }
                                }
                            }
                        }
                    }
                });
            }

            protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default)
            {
                var items = new List<string>();
                for (var i = 0; i < _itemCount; i++)
                {
                    items.Add(i.ToString());
                }

                return await innerDc.BeginDialogAsync("doItems", new { Items = items }, cancellationToken);
            }
        }
            
        [Fact]
        public void AdaptiveDialog_GetInternalVersion()
        {
            var ds = new TestAdaptiveDialog();
            var version1 = ds.GetInternalVersion_Test();
            Assert.NotNull(version1);

            var version2 = ds.GetInternalVersion_Test();
            Assert.NotNull(version2);
            Assert.Equal(version1, version2);

            var ds2 = new TestAdaptiveDialog()
            {
                Triggers = new List<OnCondition>()
                {
                    new OnIntent("foo")
                    {
                    }
                }
            };

            var version3 = ds2.GetInternalVersion_Test();
            Assert.NotNull(version3);
            Assert.NotEqual(version2, version3);

            ds2 = new TestAdaptiveDialog()
            {
                Triggers = new List<OnCondition>()
                {
                    new OnIntent("foo")
                    {
                        Condition = "user.name == 'joe'"
                    }
                }
            };

            var version4 = ds2.GetInternalVersion_Test();
            Assert.NotNull(version4);
            Assert.NotEqual(version3, version4);

            var ds3 = new TestAdaptiveDialog()
            {
                Triggers = new List<OnCondition>()
                {
                    new OnIntent("foo")
                    {
                        Condition = "user.name == 'joe'",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity() { Activity = new ActivityTemplate("yo") }
                        }
                    }
                }
            };

            var version5 = ds3.GetInternalVersion_Test();
            Assert.NotNull(version5);
            Assert.NotEqual(version4, version5);

            var version6 = ds3.GetInternalVersion_Test();
            Assert.Equal(version5, version6);
        }

        [Fact]
        public async Task AdaptiveDialog_ChangeDetect_None()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var storage = new MemoryStorage();
            var adapter = new TestAdapter()
                .UseStorage(storage)
                .UseBotState(new UserState(storage), new ConversationState(storage));

            var dm1 = new DialogManager(CreateDialog("test"));
            var dm2 = new DialogManager(CreateDialog("test"));
            var dialogManager = dm1;
            await new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
            {
                await dialogManager.OnTurnAsync(turnContext, cancellationToken);
                dialogManager = dm2;
            })
                .Send("hello")
                    .AssertReply("test")
                .Send("hello")
                    .AssertReply("test")
                .StartTestAsync();
        }

        [Fact]
        public async Task AdaptiveDialog_ChangeDetect()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var storage = new MemoryStorage();
            var adapter = new TestAdapter()
                .UseStorage(storage)
                .UseBotState(new UserState(storage), new ConversationState(storage));

            var dm1 = new DialogManager(CreateDialog("test"));
            var dm2 = new DialogManager(CreateDialog("test2"));
            var dialogManager = dm1;
            await new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
            {
                await dialogManager.OnTurnAsync(turnContext, cancellationToken);
                dialogManager = dm2;
            })
                .Send("hello")
                    .AssertReply("test")
                .Send("hello")
                    .AssertReply("changed")
                .Send("hello")
                    .AssertReply("test2")
                .StartTestAsync();
        }

        [Fact]
        public async Task AdaptiveDialog_CustomAttachmentInputDialog_NoFile()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var storage = new MemoryStorage();
            var adapter = new TestAdapter()
                .UseStorage(storage)
                .UseBotState(new UserState(storage), new ConversationState(storage));

            var dialogManager = new DialogManager(CreateDialogWithCustomInput());

            await new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
            {
                await dialogManager.OnTurnAsync(turnContext, cancellationToken);
            })
                .Send("hello")
                    .AssertReply("Upload picture")
                .Send("skip")
                    .AssertReply("passed")
                .StartTestAsync();
        }

        [Fact]
        public async Task AdaptiveDialog_CustomAttachmentInputDialog_File()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var storage = new MemoryStorage();
            var adapter = new TestAdapter()
                .UseStorage(storage)
                .UseBotState(new UserState(storage), new ConversationState(storage));

            var dialogManager = new DialogManager(CreateDialogWithCustomInput());

            var attachment = new Attachment("image/png", "https://contenturl.com", name: "image.png");
            var pictureActivity = MessageFactory.Attachment(attachment);
            await new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
            {
                await dialogManager.OnTurnAsync(turnContext, cancellationToken);
            })
                .Send("hello")
                    .AssertReply("Upload picture")
                .Send(pictureActivity)
                    .AssertReply("passed")
                .StartTestAsync();
        }

        [Fact]
        public async Task AdaptiveDialog_ReplaceParent()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var storage = new MemoryStorage();
            var adapter = new TestAdapter()
                .UseStorage(storage)
                .UseBotState(new UserState(storage), new ConversationState(storage));

            var rootDialog = new AdaptiveDialog("root")
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Replacing this dialog with a child"),
                            new ReplaceDialog()
                            {
                                Dialog = "newDialog"
                            },
                            new SendActivity("You should not see these actions since this dialog has been replaced!")
                        }
                    }
                }
            };

            var newDialog = new AdaptiveDialog("newDialog")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("This dialog (newDialog) will end after this message"),
                        }
                    }
                },
                AutoEndDialog = false
            };

            var dialogManager = new DialogManager(rootDialog);
            dialogManager.Dialogs.Add(rootDialog);
            dialogManager.Dialogs.Add(newDialog);

            await new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
            {
                await dialogManager.OnTurnAsync(turnContext, cancellationToken);
            })
                .Send("hello")
                    .AssertReply("Replacing this dialog with a child")
                    .AssertReply("This dialog (newDialog) will end after this message")
                .StartTestAsync();
        }

        [Fact]
        public async Task AdaptiveDialog_ReplaceParentComplex_VerifyPostReplace()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var storage = new MemoryStorage();
            var adapter = new TestAdapter()
                .UseStorage(storage)
                .UseBotState(new UserState(storage), new ConversationState(storage));

            var outderDialog = new AdaptiveDialog("outer")
            {
                AutoEndDialog = false,

                //Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern()
                        {
                            Intent = "start",
                            Pattern = "start"
                        },
                        new IntentPattern()
                        {
                            Intent = "where",
                            Pattern = "where"
                        }
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Say 'start' to get started")
                        }
                    },

                    // joke is always available if it is an interruption.
                    new OnIntent()
                    {
                        Intent = "start",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Starting child dialog"),
                            new BeginDialog()
                            {
                                Dialog = "root"
                            },
                            new SendActivity("child dialog has ended and returned back")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "where",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("outer dialog..")
                        }
                    }
                }
            };

            var rootDialog = new AdaptiveDialog("root")
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Replacing this dialog with a child"),
                            new ReplaceDialog()
                            {
                                Dialog = "newDialog"
                            },
                            new SendActivity("You should not see these actions since this dialog has been replaced!")
                        }
                    }
                }
            };

            var newDialog = new AdaptiveDialog("newDialog")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("This dialog (newDialog) will end after this message")
                        }
                    }
                }
            };

            var dialogManager = new DialogManager(outderDialog);
            dialogManager.Dialogs.Add(rootDialog);
            dialogManager.Dialogs.Add(newDialog);

            await new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
            {
                await dialogManager.OnTurnAsync(turnContext, cancellationToken);
            })
                .Send("hello")
                    .AssertReply("Say 'start' to get started")
                .Send("where")
                    .AssertReply("outer dialog..")
                .Send("start")
                    .AssertReply("Starting child dialog")
                    .AssertReply("Replacing this dialog with a child")
                    .AssertReply("This dialog (newDialog) will end after this message")
                    .AssertReply("child dialog has ended and returned back")
                 .Send("where")
                    .AssertReply("outer dialog..")
                .StartTestAsync();
        }

        private static AdaptiveDialog CreateDialog(string custom)
        {
            return new AdaptiveDialog()
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnDialogEvent(DialogEvents.VersionChanged)
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("changed")
                        }
                    },
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity(custom)
                        }
                    }
                }
            };
        }

        private static AdaptiveDialog CreateDialogWithCustomInput()
        {
            return new AdaptiveDialog()
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>()
                {
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
                        {
                            new AttachmentOrNullInput
                            {
                                Prompt = new ActivityTemplate("Upload picture"),
                                InvalidPrompt = new ActivityTemplate("Invalid"),
                                Validations = new List<BoolExpression>
                                {
                                    // We provide two options for the user:
                                    //   1) no attachment uploaded (skip)
                                    //   2) an attachment upload of type png or jpeg
                                    "(turn.activity.attachments == null || turn.activity.attachments.count == 0) || (turn.activity.attachments[0].contentType == 'image/jpeg' || turn.activity.attachments[0].contentType == 'image/png')",
                                },
                                Property = "user.picture"
                            },
                            new SendActivity("passed"),
                        }
                    }
                }
            };
        }

        public class AttachmentOrNullInput : AttachmentInput
        {
            public AttachmentOrNullInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
                : base(callerPath, callerLine)
            {
            }

            protected override Task<InputState> OnRecognizeInputAsync(DialogContext dc, CancellationToken cancellationToken = default)
            {
                var input = dc.State.GetValue<List<Attachment>>(VALUE_PROPERTY);
                var first = input.Count > 0 ? input[0] : null;

                // NOTE: this custom AttachmentInput allows for no attachment.
                //if (first == null || (string.IsNullOrEmpty(first.ContentUrl) && first.Content == null))
                //{
                //    return Task.FromResult(InputState.Unrecognized);
                //}

                switch (this.OutputFormat.GetValue(dc.State))
                {
                    case AttachmentOutputFormat.All:
                        dc.State.SetValue(VALUE_PROPERTY, input);
                        break;
                    case AttachmentOutputFormat.First:
                        dc.State.SetValue(VALUE_PROPERTY, first);
                        break;
                }

                return Task.FromResult(InputState.Valid);
            }
        }

        public class TestAdaptiveDialog : AdaptiveDialog
        {
            public string GetInternalVersion_Test()
            {
                EnsureDependenciesInstalled();
                return GetInternalVersion();
            }
        }
    }
}
