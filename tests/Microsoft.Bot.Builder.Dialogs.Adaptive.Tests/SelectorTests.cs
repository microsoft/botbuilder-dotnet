using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class SelectorTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task AdaptiveFirstSelector() =>
            await CreateFlow(new FirstSelector())
                .Test("trigger", "default")
                .Send("a")
                .Test("trigger", "ruleA1")
                .Send("b")
                .Test("trigger", "ruleA1")
                .Send("c")
                .Test("trigger", "ruleA1")
                .StartTestAsync();

        [TestMethod]
        public async Task AdaptiveRandomSelector() =>
            await CreateFlow(new RandomSelector())
                .Test("trigger", "default")
                .Send("a")
                .Send("trigger")
                .AssertReplyOneOf(new string[] { "default", "ruleA1", "ruleA2" })
                .Send("b")
                .Send("trigger")
                .AssertReplyOneOf(new string[] { "default", "ruleA1", "ruleA2", "ruleBorC", "ruleAandB" })
                .Send("c")
                .Send("trigger")
                .AssertReplyOneOf(new string[] { "default", "ruleA1", "ruleA2", "ruleBorC", "ruleAandB", "ruleAandC" })
                .StartTestAsync();

        [TestMethod]
        public async Task MostSpecificFirstSelector() =>
            await CreateFlow(new MostSpecificSelector())
                .Test("trigger", "default")
                .Send("a")
                .Test("trigger", "ruleA1")
                .Send("b")
                .Test("trigger", "ruleAandB")
                .Send("c")
                .Test("trigger", "ruleAandB")
                .StartTestAsync();

        [TestMethod]
        public async Task MostSpecificRandomSelector() =>
            await CreateFlow(new MostSpecificSelector() { Selector = new RandomSelector() })
                .Test("trigger", "default")
                .Send("a")
                .Send("trigger")
                .AssertReplyOneOf(new string[] { "ruleA1", "ruleA2" })
                .Send("b")
                .Send("trigger")
                .AssertReplyOneOf(new string[] { "ruleAandB" })
                .Send("c")
                .Send("trigger")
                .AssertReplyOneOf(new string[] { "ruleAandB", "ruleAandC" })
                .StartTestAsync();

        private TestFlow CreateFlow(ITriggerSelector selector)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);
            var resourceExplorer = new ResourceExplorer();
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .UseResourceExplorer(resourceExplorer)
                .UseAdaptiveDialogs()
                .UseLanguageGeneration(resourceExplorer)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var dialog = new AdaptiveDialog() { Selector = selector };
            dialog.Recognizer = new RegexRecognizer
            {
                Intents = new List<IntentPattern>()
                {
                    new IntentPattern("a", "a"),
                    new IntentPattern("b", "b"),
                    new IntentPattern("trigger", "trigger"),
                }
            };
            dialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnIntent("a", actions: new List<Dialog> { new SetProperty { Property = "user.a", Value = "1" } }),
                new OnIntent("b", actions: new List<Dialog> { new SetProperty { Property = "user.b", Value = "1" } }),
                new OnIntent("trigger", constraint: "user.a == 1", actions: new List<Dialog> { new SendActivity("ruleA1") }),
                new OnIntent("trigger", constraint: "user.a == 1", actions: new List<Dialog> { new SendActivity("ruleA2") }),
                new OnIntent("trigger", constraint: "user.b == 1 || user.c == 1", actions: new List<Dialog> { new SendActivity("ruleBorC") }),
                new OnIntent("trigger", constraint: "user.a == 1 && user.b == 1", actions: new List<Dialog> { new SendActivity("ruleAandB") }),
                new OnIntent("trigger", constraint: "user.a == 1 && user.c == 1", actions: new List<Dialog> { new SendActivity("ruleAandC") }),
                new OnIntent("trigger", constraint: string.Empty, actions: new List<Dialog> { new SendActivity("default") })
            });
            dialog.AutoEndDialog = false;

            DialogManager dm = new DialogManager(dialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }
    }
}
