// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1204 // Static elements should appear before instance elements
#pragma warning disable SA1210 // Using directives should be ordered alphabetically by namespace
#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1201 // Elements should appear in the correct order

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Xunit;

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
        public async Task TestBindingTwoWayAcrossAdaptiveDialogs()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task TestForeachWithPrompt()
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

        [Fact(Skip = "Ignore")]
        public async Task TestForeachWithLargeItems()
        {
            var testFlow = new TestScript()
            {
                Dialog = new ForeachItemsDialog()
            }
            .SendConversationUpdate();

            for (var i = 0; i < 1000; i++)
            {
                testFlow = testFlow.AssertReply(i.ToString());
            }

            await testFlow.ExecuteAsync(_resourceExplorerFixture.ResourceExplorer);
        }

        private class ForeachItemsDialog : ComponentDialog
        {
            internal ForeachItemsDialog()
            {
                AddDialog(new AdaptiveDialog
                {
                    Id = "doItems",
                    Triggers = new List<OnCondition>
                    {
                        new OnBeginDialog
                        {
                            Actions = new List<Dialog>
                            {
                                new Foreach
                                {
                                    ItemsProperty = "$items",
                                    Actions = new List<Dialog> { new SendActivity { Activity = new ActivityTemplate("${$foreach.value}") } }
                                }
                            }
                        }
                    }
                });
            }

            protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default)
            {
                var items = new List<string>();
                for (var i = 0; i < 1000; i++)
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
