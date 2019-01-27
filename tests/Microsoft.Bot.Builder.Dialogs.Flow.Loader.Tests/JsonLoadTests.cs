using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Types;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Tests.Recognizers;

namespace Microsoft.Bot.Builder.Dialogs.Loader.Tests
{
    [TestClass]
    public class JsonLoadTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Factory.Reset();
        }

        public TestContext TestContext { get; set; }

        /// <summary>
        /// An intent command dialog that has no inner dialogs. Only commands and a
        /// rule based simple recognizer that is defined inline
        /// </summary>
        [TestMethod]
        public async Task JsonDialogLoad_IntentCommandDialog_OnlyCommands()
        {
            string json = File.ReadAllText("TestFlows/IntentCommandDialog_OnlyCommands.json");

            Factory.Register("http://schemas.botframework.com/RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("hello")
            .AssertReply("Sorry, I didn't get that") // The json defines no rule for hello, so it hits the None rule
            .Send("hi")
            .AssertReply("Howdy!") // Greeting rule
            .Send("help")
            .AssertReply("I can greet and give help. Say 'hi' and I will greet you back") // Help rule
            .StartTestAsync();
        }

        /// <summary>
        /// An intent command dialog that has two inner dialogs and commands with CallDialog.
        /// </summary>
        [TestMethod]
        public async Task JsonDialogLoad_IntentCommandDialog_WithChildPrompts()
        {
            string json = File.ReadAllText("TestFlows/IntentCommandDialog_WithChildPrompts.json");

            Factory.Register("http://schemas.botframework.com/RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("yo")
            .AssertReply("Sorry, I didn't get that") // The json defines no rule for hello, so it hits the None rule
            .Send("name")
            .AssertReply("What is your name?") 
            .Send("Carlos")
            .StartTestAsync();
        }


        /// <summary>
        /// An intent dialog that with a simple recognizer and two child dialogs.
        /// </summary>
        [TestMethod]
        public async Task JsonDialogLoad_IntentDialog()
        {
            string json = File.ReadAllText("TestFlows/IntentDialog.json");

            Factory.Register("http://schemas.botframework.com/RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("name")
            .AssertReply("What is your name?")
            .Send("Carlos")
            .Send("age")
            .AssertReply("What is your age?") 
            .StartTestAsync();
        }

        /// <summary>
        /// A workflow style command dialog defined with 2 nodes, each with one dialog and
        /// several commands
        /// </summary>
        [TestMethod]
        public async Task JsonDialogLoad_CommandDialog_WithImplicitArrayCommandSet()
        {
            string json = File.ReadAllText("TestFlows/CommandDialog_WithImplicitArrayCommandSet.json");

            await BuildTestFlow(json)
            .Send("hello")
            .AssertReply("What is your name?")
            .Send("x")
            .AssertReply("What is your name?") // Should reprompt since we are validating length(name) > 2
            .Send("Joe")
            .AssertReply("What is your age?")
            .Send("whassssuuuupp")
            .AssertReply("Reprompt: What is your age?") // Should reprompt since the age provided was not numeric
            .Send("64")
            .AssertReply("Done")
            .StartTestAsync();
        }

        /// <summary>
        /// A workflow style command dialog defined with 2 nodes, each with one dialog and
        /// several commands
        /// </summary>
        [TestMethod]
        public async Task JsonDialogLoad_CommandDialog()
        {
            string json = File.ReadAllText("TestFlows/CommandDialog.json");

            await BuildTestFlow(json)
            .Send("hello")
            .AssertReply("What is your name?")
            .Send("x") 
            .AssertReply("What is your name?") // Should reprompt since we are validating length(name) > 2
            .Send("Joe")
            .AssertReply("What is your age?")
            .Send("whassssuuuupp") 
            .AssertReply("Reprompt: What is your age?") // Should reprompt since the age provided was not numeric
            .Send("64")
            .AssertReply("Done")
            .StartTestAsync();
        }

        private TestFlow BuildTestFlow(string json)
        {
            var dialog = DialogLoader.Load(json);

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger()))
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            dialogs.Add(dialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());

                var dialogContext = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dialogContext.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty || results.Status == DialogTurnStatus.Complete)
                    results = await dialogContext.BeginDialogAsync(dialog.Id, null, cancellationToken);
            });
        }
    }
}
