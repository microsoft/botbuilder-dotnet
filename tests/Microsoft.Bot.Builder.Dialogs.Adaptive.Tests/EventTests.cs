// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Events;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class EventTests
    {
        public TestContext TestContext { get; set; }

        public ExpressionEngine ExpressionEngine { get; set; } = new ExpressionEngine();

        [TestMethod]
        public async Task EventTests_OnIntent()
        {
            var planningDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        { "JokeIntent", "joke" }
                    }
                },
                Events = new List<IOnEvent>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<IDialog>()
                        {
                            new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                        },
                    },
                    new OnIntent(
                        "JokeIntent",
                        actions: new List<IDialog>()
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
        public async Task EventTests_OnIntentWithEntities()
        {
            var planningDialog = new AdaptiveDialog("planningTest")
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        { "addColor", "I want (?<color>(red|green|blue|yellow))*" }
                    }
                },
                Events = new List<IOnEvent>()
                {
                    new OnIntent(
                        intent: "addColor",
                        entities: new List<string>() { "color" },
                        actions: new List<IDialog>() { new SendActivity("You picked {@color}") }),
                    new OnUnknownIntent(actions: new List<IDialog>() { new SendActivity("pbtpbtpbt!") })
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
