// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
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
            var planningDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("JokeIntent", "joke"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                        },
                    },
                    new OnIntent(
                        "JokeIntent",
                        actions: new List<Dialog>()
                        {
                            new SendActivity("Why did the chicken cross the road?"),
                            new EndTurn(),
                            new SendActivity("To get to the other side")
                        }),
                }
            };

            await CreateFlow(planningDialog)
            .SendConversationUpdate()
                .AssertReply("I'm a joke bot. To get started say 'tell me a joke'")
            .Send("Do you know a joke?")
                .AssertReply("Why did the chicken cross the road?")
            .Send("Why?")
                .AssertReply("To get to the other side")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task OnIntentWithEntities()
        {
            var planningDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern("addColor", "I want (?<color>(red|green|blue|yellow))*"),
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent(
                        intent: "addColor",
                        entities: new List<string>() { "color" },
                        actions: new List<Dialog>() { new SendActivity("You picked {@color}") }),
                    new OnUnknownIntent(actions: new List<Dialog>() { new SendActivity("pbtpbtpbt!") })
                }
            };

            await CreateFlow(planningDialog)
            .Send("I want red")
                .AssertReply("You picked red")
            .Send("I want")
                .AssertReply("pbtpbtpbt!")
            .Send("fooo")
                .AssertReply("pbtpbtpbt!")
            .StartTestAsync();
        }

        public OnCondition TestCondition(OnCondition conditional)
        {
            conditional.Condition = $"turn.activity.text == '{conditional.GetType().Name}'";
            conditional.Actions.Add(new SendActivity(conditional.GetType().Name));
            return conditional;
        }

        [TestMethod]
        public async Task OnActivityTypes()
        {
            var planningDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>()
                {
                    TestCondition(new OnMessageActivity()),
                    TestCondition(new OnEventActivity()),
                    TestCondition(new OnConversationUpdateActivity()),
                    TestCondition(new OnTypingActivity()),
                    TestCondition(new OnEndOfConversationActivity()),
                    TestCondition(new OnEventActivity()),
                    TestCondition(new OnHandoffActivity()),
                    TestCondition(new OnMessageReactionActivity()),
                    TestCondition(new OnMessageUpdateActivity()),
                    TestCondition(new OnMessageDeleteActivity()),
                }
            };

            await CreateFlow(planningDialog)
            .Send(new Activity(ActivityTypes.Message, text: nameof(OnMessageActivity)))
                .AssertReply(nameof(OnMessageActivity))
            .Send(new Activity(ActivityTypes.MessageReaction, text: nameof(OnMessageReactionActivity)))
                .AssertReply(nameof(OnMessageReactionActivity))
            .Send(new Activity(ActivityTypes.MessageDelete, text: nameof(OnMessageDeleteActivity)))
                .AssertReply(nameof(OnMessageDeleteActivity))
            .Send(new Activity(ActivityTypes.MessageUpdate, text: nameof(OnMessageUpdateActivity)))
                .AssertReply(nameof(OnMessageUpdateActivity))
            .Send(new Activity(ActivityTypes.Typing, text: nameof(OnTypingActivity)))
                .AssertReply(nameof(OnTypingActivity))
            .Send(new Activity(ActivityTypes.ConversationUpdate, text: nameof(OnConversationUpdateActivity)))
                .AssertReply(nameof(OnConversationUpdateActivity))
            .Send(new Activity(ActivityTypes.EndOfConversation, text: nameof(OnEndOfConversationActivity)))
                .AssertReply(nameof(OnEndOfConversationActivity))
            .Send(new Activity(ActivityTypes.Event, text: nameof(OnEventActivity)) { Name = nameof(OnEventActivity) })
                .AssertReply(nameof(OnEventActivity))
            .StartTestAsync();
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

        private TestFlow CreateFlow(AdaptiveDialog ruleDialog)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();

            var explorer = new ResourceExplorer();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .Use(new RegisterClassMiddleware<ResourceExplorer>(explorer))
                .UseAdaptiveDialogs()
                .UseLanguageGeneration(explorer)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            DialogManager dm = new DialogManager(ruleDialog);
            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }
    }
}
