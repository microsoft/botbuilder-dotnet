// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dbg = System.Diagnostics;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class ConditionalsTests
    {
        public TestContext TestContext { get; set; }

        public ExpressionEngine ExpressionEngine { get; set; } = new ExpressionEngine();

        [TestMethod]
        public async Task OnIntent()
        {
            await TestUtils.RunTestScript("ConditionalsTests_OnIntent.test.dialog");
        }

        [TestMethod]
        public async Task OnIntentWithEntities()
        {
            await TestUtils.RunTestScript("ConditionalsTests_OnIntentWithEntities.test.dialog");
        }

        [TestMethod]
        public async Task OnActivityTypes()
        {
            await TestUtils.RunTestScript("ConditionalsTests_OnActivityTypes.test.dialog");
        }

        public void AssertExpression(OnCondition condition, string expectedExpression)
        {
            var exp = condition.GetExpression(new ExpressionEngine());
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
                new OnCustomEvent()
                {
                    Event = "CustomEvent",
                    Condition = "turn.test == 1"
                },
                "((turn.dialogEvent.name == 'CustomEvent') && (turn.test == 1))");

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
