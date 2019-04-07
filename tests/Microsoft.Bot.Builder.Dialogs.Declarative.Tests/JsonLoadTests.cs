// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Dialogs.Declarative.Tests.Recognizers;
using Microsoft.Bot.Builder.AI.LanguageGeneration;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.Dialogs.Declarative.Debugger;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Dialogs.Loader.Tests
{
    [TestClass]
    public class JsonLoadTests
    {
        private readonly string samplesDirectory = @"..\..\..\..\..\samples\Microsoft.Bot.Builder.TestBot.Json\Samples\";

        [ClassInitialize]
        public static void ClassInitilize(TestContext context)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            TypeFactory.RegisterAdaptiveTypes();
            TypeFactory.Register("Microsoft.RuleRecognizer", typeof(RuleRecognizer));
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task JsonDialogLoad_NoMatchRule()
        {
            string path = Path.Combine(samplesDirectory, @"Planning 1 - NoMatchRule\NoMatchRule.main.dialog");

            await BuildTestFlow(path)
            .Send("hello")
            .AssertReply("Hello planning!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_EndTurn()
        {
            string path = Path.Combine(samplesDirectory, @"Planning 2 - EndTurn\EndTurn.main.dialog");

            await BuildTestFlow(path)
            .Send("hello")
            .AssertReply("What's up?")
            .Send("Nothing")
            .AssertReply("Oh I see!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_IfProperty()
        {
            string path = Path.Combine(samplesDirectory, @"Planning 3 - IfCondition\IfCondition.main.dialog");

            await BuildTestFlow(path)
            .Send("hello")
            .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
            .AssertReply("Hello Carlos, nice to talk to you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_TextInput()
        {
            string path = Path.Combine(samplesDirectory, @"Planning 4 - TextInput\TextInput.main.dialog");

            await BuildTestFlow(path)
            .Send("hello")
            .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
            .AssertReply("Hello Carlos, nice to talk to you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_WelcomePrompt()
        {
            string path = Path.Combine(samplesDirectory, @"Planning 5 - WelcomeRule\WelcomeRule.main.dialog");

            await BuildTestFlow(path)
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
            string path = Path.Combine(samplesDirectory, @"Planning 6 - DoSteps\DoSteps.main.dialog");

            await BuildTestFlow(path)
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
        public async Task JsonDialogLoad_BeginDialog()
        {
            string path = Path.Combine(samplesDirectory, @"Planning 7 - BeginDialog\BeginDialog.main.dialog");

            await BuildTestFlow(path)
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
            .AssertReply("I see great things in your future...")
            .AssertReply("Potentially a successful demo")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_ChoiceInputDialog()
        {
            string path = Path.Combine(samplesDirectory, @"Planning 10 - ChoiceInput\ChoiceInput.main.dialog");

            await BuildTestFlow(path)
            .Send(new Activity(ActivityTypes.ConversationUpdate, membersAdded: new List<ChannelAccount>() { new ChannelAccount("bot", "Bot") }))
            .Send("hello")
            .AssertReply("Please select a value from below:\n\n   1. Test1\n   2. Test2\n   3. Test3")
            .Send("Test1")
            .AssertReply("You select: Test1")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_ExternalLanguage()
        {
            string path = Path.Combine(samplesDirectory, @"Planning 8 - ExternalLanguage\ExternalLanguage.main.dialog");

            await BuildTestFlow(path)
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
            .AssertReply("I can tell jokes and also forsee the future!\n")
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
        public async Task JsonDialogLoad_ToDoBot()
        {
            string path = Path.Combine(samplesDirectory, @"Planning - ToDoBot\TodoBot.main.dialog");

            await BuildTestFlow(path)
            .Send(new Activity(ActivityTypes.ConversationUpdate, membersAdded: new List<ChannelAccount>() { new ChannelAccount("bot", "Bot") }))
            .Send("hello")
            .AssertReply("Hi! I'm a ToDo bot. Say \"add a todo named first\" to get started.")
            .Send("add a todo named first")
            .AssertReply("Successfully added a todo named \"first\"")
            .Send("add a todo named second")
            .AssertReply("Successfully added a todo named \"second\"")
            .Send("add a todo")
            .AssertReply("OK, please enter the title of your todo.")
            .Send("third")
            .AssertReply("Successfully added a todo named \"third\"")
            .Send("show todos")
            .AssertReply("Your most recent 3 tasks are\n* first\n* second\n* third\n")
            .Send("delete todo named second")
            .AssertReply("Successfully removed a todo named \"second\"")
            .Send("show todos")
            .AssertReply("Your most recent 2 tasks are\n* first\n* third\n")
            .Send("add a todo")
            .AssertReply("OK, please enter the title of your todo.")
            .Send("cancel")
            .AssertReply("ok.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_HttpRequest()
        {
            string path = Path.Combine(samplesDirectory, @"Planning 11 - HttpRequest\HttpRequest.main.dialog");

            await BuildTestFlow(path)
            .Send(new Activity(ActivityTypes.ConversationUpdate, membersAdded: new List<ChannelAccount>() { new ChannelAccount("bot", "Bot") }))
            .AssertReply("Welcome! Here is a http request sample, please enter a name for you visual pet.")
            .Send("TestPetName")
            .AssertReply("Great! Your pet's name is TestPetName")
            .AssertReply("Now please enter the id of your pet, this could help you find your pet later.")
            .Send("12121")
            .AssertReply("Done! You have added a pet named \"TestPetName\" with id \"12121\"")
            .AssertReply("Now try to specify the id of your pet, and I will help your find it out from the store.")
            .Send("12121")
            .AssertReply("Great! I found your pet named \"TestPetName\"")
            .StartTestAsync();
        }

        private TestFlow BuildTestFlow(string path)
        {
            string projPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $@"..\..\..\..\..\samples\Microsoft.Bot.Builder.TestBot.Json\Microsoft.Bot.Builder.TestBot.Json.csproj"));
            var resourceExplorer = ResourceExplorer.LoadProject(projPath);

            var dialog = DeclarativeTypeLoader.Load<IDialog>(path, resourceExplorer, Source.NullRegistry.Instance);

            IStorage dataStore = new MemoryStorage();

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var lg = new LGLanguageGenerator(resourceExplorer);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new RegisterClassMiddleware<ResourceExplorer>(resourceExplorer))
                .Use(new RegisterClassMiddleware<ILanguageGenerator>(lg))
                .Use(new RegisterClassMiddleware<IMessageActivityGenerator>(new TextMessageActivityGenerator(lg)))
                .Use(new RegisterClassMiddleware<IStorage>(dataStore));

            var dialogs = new DialogSet(dialogState);

            dialogs.Add(dialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                if (dialog is AdaptiveDialog planningDialog)
                {
                    await planningDialog.OnTurnAsync(turnContext, null, cancellationToken).ConfigureAwait(false);
                }
            });
        }
    }
}
