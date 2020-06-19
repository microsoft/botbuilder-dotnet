// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dbg = System.Diagnostics;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class ConditionalsTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(ConditionalsTests)), monitorChanges: false);
        }
        
        [TestMethod]
        public async Task ConditionalsTests_OnIntent()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task ConditionalsTests_OnIntentWithEntities()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task ConditionalsTests_OnActivityTypes()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task ConditionalsTests_OnChooseIntent()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        public void AssertExpression(OnCondition condition, string expectedExpression)
        {
            var exp = condition.GetExpression();
            dbg.Trace.TraceInformation(exp.ToString());
            Assert.AreEqual(expectedExpression, exp.ToString());
        }

        [TestMethod]
        public void OnConditionWithCondition()
        {
            AssertExpression(
                new OnMessageActivity()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.activity.type == '{ActivityTypes.Message}') && ((turn.dialogEvent.name == '{AdaptiveEvents.ActivityReceived}') && (turn.test == 1)))");

            AssertExpression(
                new OnEventActivity()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.activity.type == '{ActivityTypes.Event}') && ((turn.dialogEvent.name == '{AdaptiveEvents.ActivityReceived}') && (turn.test == 1)))");

            AssertExpression(
                new OnConversationUpdateActivity()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.activity.type == '{ActivityTypes.ConversationUpdate}') && ((turn.dialogEvent.name == '{AdaptiveEvents.ActivityReceived}') && (turn.test == 1)))");

            AssertExpression(
                new OnTypingActivity()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.activity.type == '{ActivityTypes.Typing}') && ((turn.dialogEvent.name == '{AdaptiveEvents.ActivityReceived}') && (turn.test == 1)))");

            AssertExpression(
                new OnEndOfConversationActivity()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.activity.type == '{ActivityTypes.EndOfConversation}') && ((turn.dialogEvent.name == '{AdaptiveEvents.ActivityReceived}') && (turn.test == 1)))");

            AssertExpression(
                new OnEventActivity()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.activity.type == '{ActivityTypes.Event}') && ((turn.dialogEvent.name == '{AdaptiveEvents.ActivityReceived}') && (turn.test == 1)))");

            AssertExpression(
                new OnHandoffActivity()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.activity.type == '{ActivityTypes.Handoff}') && ((turn.dialogEvent.name == '{AdaptiveEvents.ActivityReceived}') && (turn.test == 1)))");

            AssertExpression(
                new OnMessageReactionActivity()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.activity.type == '{ActivityTypes.MessageReaction}') && ((turn.dialogEvent.name == '{AdaptiveEvents.ActivityReceived}') && (turn.test == 1)))");

            AssertExpression(
                new OnMessageUpdateActivity()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.activity.type == '{ActivityTypes.MessageUpdate}') && ((turn.dialogEvent.name == '{AdaptiveEvents.ActivityReceived}') && (turn.test == 1)))");

            AssertExpression(
                new OnMessageDeleteActivity()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.activity.type == '{ActivityTypes.MessageDelete}') && ((turn.dialogEvent.name == '{AdaptiveEvents.ActivityReceived}') && (turn.test == 1)))");

            AssertExpression(
                new OnIntent()
                {
                    Intent = "Intent",
                    Entities = new List<string>() { "@foo", "@@bar", "turn.recognized.entities.blat", "gronk" },
                    Condition = "turn.test == 1"
                },
                $"(((turn.recognized.intent == 'Intent') && (exists(@foo) && exists(@@bar) && exists(turn.recognized.entities.blat) && exists(@gronk))) && ((turn.dialogEvent.name == '{AdaptiveEvents.RecognizedIntent}') && (turn.test == 1)))");

            AssertExpression(
                new OnUnknownIntent()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.dialogEvent.name == '{AdaptiveEvents.UnknownIntent}') && (turn.test == 1))");

            AssertExpression(
                new OnBeginDialog()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.dialogEvent.name == '{DialogEvents.BeginDialog}') && (turn.test == 1))");

            AssertExpression(
                new OnCancelDialog()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.dialogEvent.name == '{DialogEvents.CancelDialog}') && (turn.test == 1))");

            AssertExpression(
                new OnRepromptDialog()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.dialogEvent.name == '{DialogEvents.RepromptDialog}') && (turn.test == 1))");

            AssertExpression(
                new OnError()
                {
                    Condition = "turn.test == 1"
                },
                $"((turn.dialogEvent.name == '{DialogEvents.Error}') && (turn.test == 1))");

            AssertExpression(
                new OnDialogEvent()
                {
                    Event = "DialogEvent",
                    Condition = "turn.test == 1"
                },
                "((turn.dialogEvent.name == 'DialogEvent') && (turn.test == 1))");

            AssertExpression(
                new OnCondition()
                {
                    Condition = "turn.test == 1"
                },
                "(turn.test == 1)");
        }
    }
}
