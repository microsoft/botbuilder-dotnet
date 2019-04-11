using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class SelectorTests
    {
        public TestContext TestContext { get; set; }

        private TestFlow CreateFlow(IRuleSelector selector)
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());
            var planningDialog = new AdaptiveDialog() { Selector = selector };
            planningDialog.Recognizer = new RegexRecognizer
            {
                Intents = new Dictionary<string, string>
                {
                    { "a", "a" },
                    { "b", "b" },
                    { "trigger", "trigger" }
                }
            };
            planningDialog.AddRules(new List<IRule>()
            {
                new IntentRule("a", steps: new List<IDialog> { new SetProperty {  OutputProperty = "user.a", Value = Expression.ConstantExpression(1) } }),
                new IntentRule("b", steps: new List<IDialog> { new SetProperty {  OutputProperty = "user.b", Value = Expression.ConstantExpression(1) } }),
                new IntentRule("trigger", constraint:"user.a == 1", steps: new List<IDialog> { new SendActivity("ruleA1") }),
                new IntentRule("trigger", constraint:"user.a == 1", steps: new List<IDialog> { new SendActivity("ruleA2") }),
                new IntentRule("trigger", constraint:"user.b == 1 || user.c == 1", steps: new List<IDialog> { new SendActivity("ruleBorC") }),
                new IntentRule("trigger", constraint:"user.a == 1 && user.b == 1", steps: new List<IDialog> { new SendActivity("ruleAandB") }),
                new IntentRule("trigger", constraint:"user.a == 1 && user.c == 1", steps: new List<IDialog> { new SendActivity("ruleAandC") }),
                new IntentRule("trigger", steps: new List<IDialog> { new SendActivity("default")})
            });
            planningDialog.AutoEndDialog = false;

            var botResourceManager = new ResourceExplorer();
            var lg = new LGLanguageGenerator(botResourceManager);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new RegisterClassMiddleware<ResourceExplorer>(botResourceManager))
                .Use(new RegisterClassMiddleware<ILanguageGenerator>(lg))
                .Use(new RegisterClassMiddleware<IStorage>(new MemoryStorage()))
                .Use(new RegisterClassMiddleware<IMessageActivityGenerator>(new TextMessageActivityGenerator(lg)))
                .Use(new AutoSaveStateMiddleware(convoState, userState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var userStateProperty = userState.CreateProperty<Dictionary<string, object>>("user");
            var convoStateProperty = convoState.CreateProperty<Dictionary<string, object>>("conversation");

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await planningDialog.OnTurnAsync(turnContext, null).ConfigureAwait(false);
            });
        }

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
    }
}
