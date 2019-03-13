using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Dialogs.Declarative.Tests.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Plugins;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.AI.LanguageGeneration;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Rules;
using System;

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

        [TestMethod]
        public async Task JsonDialogLoad_Fallback()
        {
            string json = File.ReadAllText("Samples/Planning 1 - Fallback/main.dialog");

            Factory.Register("Microsoft.RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("hello")
            .AssertReply("Hello planning!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_WaitForInput()
        {
            string json = File.ReadAllText("Samples/Planning 2 - WaitForInput/main.dialog");

            Factory.Register("Microsoft.RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("hello")
            .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
            .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_IfProperty()
        {
            string json = File.ReadAllText("Samples/Planning 3 - IfProperty/main.dialog");

            Factory.Register("Microsoft.RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("hello")
            .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
            .AssertReply("Hello Carlos, nice to talk to you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_TextPrompt()
        {
            string json = File.ReadAllText("Samples/Planning 4 - TextPrompt/main.dialog");

            Factory.Register("Microsoft.RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send("hello")
            .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
            .AssertReply("Hello Carlos, nice to talk to you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_WelcomePrompt()
        {
            string json = File.ReadAllText("Samples/Planning 5 - WelcomeRule/main.dialog");

            Factory.Register("Microsoft.RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send(new Activity(ActivityTypes.ConversationUpdate, membersAdded: new List<ChannelAccount>() { new ChannelAccount("bot", "Bot") }))
            .Send("hello")
            .AssertReply("Welcome!")
            .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
            .AssertReply("Hello Carlos, nice to talk to you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_DoSteps()
        {
            string json = File.ReadAllText("Samples/Planning 6 - DoSteps/main.dialog");

            Factory.Register("Microsoft.RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send(new Activity(ActivityTypes.ConversationUpdate, membersAdded: new List<ChannelAccount>() { new ChannelAccount("bot", "Bot") }))
            .Send("hello")
            .AssertReply("Welcome!")
            .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
            .AssertReply("Hello Carlos, nice to talk to you!")
            .Send("Do you know a joke?")
            .AssertReply("Why did the chicken cross the road?")
            .Send("Why?")
            .AssertReply("To get to the other side")
            .Send("What happened in the future?")
            .AssertReply("Seeing into the future...")
            .AssertReply("I see great things happening...")
            .AssertReply("Perhaps even a successful bot demo")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_CallDialog()
        {
            string json = File.ReadAllText("Samples/Planning 7 - CallDialog/main.dialog");

            Factory.Register("Microsoft.RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send(new Activity(ActivityTypes.ConversationUpdate, membersAdded: new List<ChannelAccount>() { new ChannelAccount("bot", "Bot") }))
            .Send("hello")
            .AssertReply("Welcome!")
            .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
            .AssertReply("Hello Carlos, nice to talk to you!")
            .Send("Do you know a joke?")
            .AssertReply("Why did the chicken cross the road?")
            .Send("Why?")
            .AssertReply("To get to the other side")
            .Send("What happened in the future?")
            .AssertReply("I see great things in your future...")
            .AssertReply("Potentially a successful demo")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_ExternalLanguage()
        {
            string json = File.ReadAllText("Samples/Planning 8 - ExternalLanguage/main.dialog");

            Factory.Register("Microsoft.RuleRecognizer", typeof(RuleRecognizer));

            await BuildTestFlow(json)
            .Send(new Activity(ActivityTypes.ConversationUpdate, membersAdded: new List<ChannelAccount>() { new ChannelAccount("bot", "Bot") }))
            .Send("hello")
            .AssertReplyOneOf(new string[]
            {
                "Zoidberg here, welcome to my world!",
                "Hello, my name is Zoidberg and I'll be your guide.",
                "Hail Zoidberg!"
            })
            .AssertReplyOneOf(new string[]
            {
                "Hello. What is your name?",
                "I would like to know you better, what's your name?"
            })
            .Send("Carlos")
            .AssertReplyOneOf(new string[]
            {
                "Hello Carlos, nice to talk to you!",
                "Hi Carlos, you seem nice!",
                "Whassup Carlos?"
            })
            .Send("Help")
            .AssertReply("I can tell jokes and also forsee the future!")
            .Send("Do you know a joke?")
            .AssertReply("Why did the chicken cross the road?")
            .Send("Why?")
            .AssertReply("To get to the other side")
            .Send("What happened in the future?")
            .AssertReply("I see great things in your future...")
            .AssertReply("Potentially a successful demo")
            .StartTestAsync();
        }

        private TestFlow BuildTestFlow(string json)
        {
            var dialog = DeclarativeTypeLoader.Load<IDialog>(json);

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            string projPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $@"..\..\..\Microsoft.Bot.Builder.Dialogs.Declarative.Tests.csproj"));
            var botResourceManager = new BotResourceManager()
                .AddProjectResources(projPath);
            var lg = new LGLanguageGenerator(botResourceManager);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new RegisterClassMiddleware<IBotResourceProvider>(botResourceManager))
                .Use(new RegisterClassMiddleware<ILanguageGenerator>(lg))
                .Use(new RegisterClassMiddleware<IMessageActivityGenerator>(new TextMessageActivityGenerator(lg)));

            var dialogs = new DialogSet(dialogState);

            dialogs.Add(dialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                if (dialog is RuleDialog planningDialog)
                {
                    await planningDialog.OnTurnAsync(turnContext, null, cancellationToken).ConfigureAwait(false);
                }
            });
        }
    }
}
