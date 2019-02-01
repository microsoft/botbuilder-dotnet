using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Types;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Tests.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Plugins;

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
        public async Task JsonDialogLoad_TextPromptWithMatchValidator()
        {
            string json = File.ReadAllText("TestFlows/TextPrompt.json");

            Factory.Register("http://schemas.botframework.com/RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("hello")
            .AssertReply("What is your name?") 
            .Send("x")
            .AssertReply("You need to give me at least 3 chars to 30 chars as a name.")
            .Send("Carlos")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_NumberPromptWithMatchValidator()
        {
            string json = File.ReadAllText("TestFlows/NumberPrompt.json");

            Factory.Register("http://schemas.botframework.com/RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("hello")
                .AssertReply("What is your age?")
            .Send("x")
                .AssertReply("I didn't recognize a number in your response.")
                .AssertReply("Let's try again, what's your age?")
            .Send("-250")
                .AssertReply("Nobody can be negative aged!")
                .AssertReply("Let's try again, what's your age?")
            .Send("250")
                .AssertReply("I don't think anyone can be that old.")
                .AssertReply("Let's try again, what's your age?")
            .Send("31")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_SequenceWithRefPrompts()
        {
            string json = File.ReadAllText("TestFlows/SequenceWithPrompts/SequenceWithRefPrompts.json");

            Factory.Register("http://schemas.botframework.com/RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("hello")
                .AssertReply("What is your name?")
            .Send("x")
                .AssertReply("You need to give me at least 3 chars to 30 chars as a name.")
            .Send("Carlos")
                .AssertReply("What is your age?")
            .Send("x")
                .AssertReply("I didn't recognize a number in your response.")
                .AssertReply("Let's try again, what's your age?")
            .Send("-250")
                .AssertReply("Nobody can be negative aged!")
                .AssertReply("Let's try again, what's your age?")
            .Send("250")
                .AssertReply("I don't think anyone can be that old.")
                .AssertReply("Let's try again, what's your age?")
            .Send("31")

            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_SequenceWithRefPromptsOverrides()
        {
            string json = File.ReadAllText("TestFlows/SequenceWithPrompts/SequenceWithRefPromptsOverrides.json");

            Factory.Register("http://schemas.botframework.com/RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("hello")
                .AssertReply("What is your name?")
            .Send("x")
                .AssertReply("You need to give me at least 3 chars to 30 chars as a name.")
            .Send("Carlos")
                .AssertReply("What is your age?")
            .Send("x")
                .AssertReply("I didn't recognize a number in your response.")
                .AssertReply("C'mon, tell me your age, I won't tell!")
            .Send("-250")
                .AssertReply("Nobody can be negative aged!")
                .AssertReply("C'mon, tell me your age, I won't tell!")
            .Send("250")
                .AssertReply("I don't think anyone can be that old.")
                .AssertReply("C'mon, tell me your age, I won't tell!")
            .Send("31")

            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_SequenceWithInlinePrompts()
        {
            string json = File.ReadAllText("TestFlows/SequenceWithPrompts/SequenceWithInlinePrompts.json");

            Factory.Register("http://schemas.botframework.com/RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("hello")
                .AssertReply("What is your name?")
            .Send("x")
                .AssertReply("You need to give me at least 3 chars to 30 chars as a name.")
            .Send("Carlos")
                .AssertReply("What is your age?")
            .Send("x")
                .AssertReply("I didn't recognize a number in your response.")
                .AssertReply("Let's try again, what's your age?")
            .Send("-250")
                .AssertReply("Nobody can be negative aged!")
                .AssertReply("Let's try again, what's your age?")
            .Send("250")
                .AssertReply("I don't think anyone can be that old.")
                .AssertReply("Let's try again, what's your age?")
            .Send("31")

            .StartTestAsync();
        }

        /// <summary>
        /// An intent dialog that with a simple recognizer and two child dialogs.
        /// </summary>
        [TestMethod]
        public async Task JsonDialogLoad_IntentDialogRelativeReferences()
        {
            string json = File.ReadAllText("TestFlows/IntentDialogRelativeReferences.json");

            Factory.Register("http://schemas.botframework.com/RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("name")
            .AssertReply("What is your name?")
            .Send("Carlos")
            .Send("age")
            .AssertReply("What is your age?") 
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_IntentDialogLuisRecognizer()
        {
            string json = File.ReadAllText("TestFlows/LuisRecognizerBasic.json");

            Factory.Register("http://schemas.botframework.com/RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("name")
            .AssertReply("What is your name?")
            .Send("Carlos")
            .Send("age")
            .AssertReply("What is your age?")
            //.Send("name")
            //.AssertReply("What is your name?")
            //.Send("Carlos")
            //.Send("name")
            //.AssertReply("What is your name?")
            //.Send("Carlos")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_FileDependencyPlugin()
        {
            string json = File.ReadAllText("TestFlows/FilePluginEchoDialog.json");

            IPlugin filePlugin = new FilePlugin(
                new FileDependencyInfo()
                {
                    AssemblyPath =
                    @"..\..\..\..\Microsoft.Bot.Builder.Dialogs.Flow.Tests\bin\Debug\netcoreapp2.1\Microsoft.Bot.Builder.Dialogs.Flows.Tests.dll",
                    ClassName = "EchoDialog",
                    SchemaUri = "custom::EchoDialog"
                });

            await Factory.RegisterPlugin(filePlugin);

            await BuildTestFlow(json)
            .Send("howdy")
            .AssertReply("howdy")
            .Send("echo")
            .AssertReply("echo")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_IntentDialogFileReferences()
        {
            string json = File.ReadAllText("TestFlows/IntentDialogRelativeReferences.json");

            Factory.Register("http://schemas.botframework.com/RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("name")
            .AssertReply("What is your name?")
            .Send("Carlos")
            .Send("age")
            .AssertReply("What is your age?")
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
