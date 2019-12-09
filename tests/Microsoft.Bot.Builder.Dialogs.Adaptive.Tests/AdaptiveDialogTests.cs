// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1204 // Static elements should appear before instance elements
#pragma warning disable SA1210 // Using directives should be ordered alphabetically by namespace
#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1208 // System using directives should be placed before other using directives

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class AdaptiveDialogTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task AdaptiveDialog_ActivityEvents()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_ActivityEvents.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_ActivityAndIntentEvents()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_ActivityAndIntentEvents.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_AdaptiveCardSubmit()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_AdaptiveCardSubmit.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_AllowInterruptionAlwaysWithFailedValidation()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_AllowInterruptionAlwaysWithFailedValidation.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_AllowInterruptionNever()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_AllowInterruptionNever.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_AllowInterruptionNeverWithInvalidInput()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_AllowInterruptionNeverWithInvalidInput.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_AllowInterruptionNeverWithMaxCount()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_AllowInterruptionNeverWithMaxCount.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_AllowInterruptionNeverWithUnrecognizedInput()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_AllowInterruptionNeverWithUnrecognizedInput.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_BeginDialog()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_BeginDialog.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_BindingCaptureValueWithinSameAdaptive()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_BindingCaptureValueWithinSameAdaptive.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_BindingOptionsAcrossAdaptiveDialogs()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_BindingOptionsAcrossAdaptiveDialogs.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_BindingReferValueInLaterAction()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_BindingReferValueInLaterAction.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_BindingReferValueInNestedAction()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_BindingReferValueInNestedAction.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_ConditionallyAllowInterruptions()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_ConditionallyAllowInterruptions.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_DoActions()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_DoActions.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_EditArray()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_EditArray.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_EndTurn()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_EndTurn.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_IfProperty()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_IfProperty.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_NestedInlineSequences()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_NestedInlineSequences.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_NestedRecognizers()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_NestedRecognizers.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_PropertySetInInterruption()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_PropertySetInInterruption.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_ReplacePlan()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_ReplacePlan.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_ReProcessInputProperty()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_ReProcessInputProperty.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_ReProcessInputPropertyValidOnlyOnce()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_ReProcessInputPropertyValidOnlyOnce.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_StringLiteralInExpression()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_StringLiteralInExpression.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_TextInput()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_TextInput.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_TextInputDefaultValueResponse()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_TextInputDefaultValueResponse.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_TextInputNoMaxTurnCount()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_TextInputNoMaxTurnCount.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_TopLevelFallback()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_TopLevelFallback.test.dialog");
        }

        [TestMethod]
        public async Task AdaptiveDialog_TopLevelFallbackMultipleActivities()
        {
            await TestUtils.RunTestScript("AdaptiveDialog_TopLevelFallbackMultipleActivities.test.dialog");
        }

        [TestMethod]
        public async Task TestBindingTwoWayAcrossAdaptiveDialogs()
        {
            await TestUtils.RunTestScript("TestBindingTwoWayAcrossAdaptiveDialogs.test.dialog");

            //await TestBindingTwoWayAcrossAdaptiveDialogs(new { userName = "$name" });
        }

        [TestMethod]
        public async Task TestForeachWithPrompt()
        {
            await TestUtils.RunTestScript("TestForeachWithPrompt.test.dialog");
        }

        [TestMethod]
        public async Task TestBindingTwoWayAcrossAdaptiveDialogsDefaultResultProperty()
        {
            await TestUtils.RunTestScript("TestBindingTwoWayAcrossAdaptiveDialogsDefaultResultProperty.test.dialog");
        }

        [TestMethod]
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

            await testFlow.ExecuteAsync();
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
                                    Actions = new List<Dialog> { new SendActivity { Activity = new ActivityTemplate("@{$foreach.value}") } }
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
    }
}
