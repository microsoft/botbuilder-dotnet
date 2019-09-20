// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Tests.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Loader.Tests
{
    [TestClass]
    public class JsonLoadTests
    {
        private static ResourceExplorer resourceExplorer;

        private readonly string samplesDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.TestBot.Json\Samples\");

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            DeclarativeTypeLoader.AddComponent(new AdaptiveComponentRegistration());
            DeclarativeTypeLoader.AddComponent(new LanguageGenerationComponentRegistration());
            TypeFactory.Register("Microsoft.RuleRecognizer", typeof(RuleRecognizer));
            string projPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath($@"..\..\..\..\..\tests\Microsoft.Bot.Builder.TestBot.Json\Microsoft.Bot.Builder.TestBot.Json.csproj")));
            resourceExplorer = ResourceExplorer.LoadProject(projPath);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            resourceExplorer.Dispose();
        }

        [TestMethod]
        public async Task JsonDialogLoad_Actions()
        {
            await BuildTestFlow(@"Actions.main.dialog")
                .SendConversationUpdate()
                .AssertReply("Action 1")
                .AssertReply("Action 2")
                .AssertReply("Action 3")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_EndTurn()
        {
            await BuildTestFlow("EndTurn.main.dialog")
            .Send("hello")
                .AssertReply("What's up?")
            .Send("Nothing")
                .AssertReply("Oh I see!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_IfProperty()
        {
            await BuildTestFlow("IfCondition.main.dialog")
            .SendConversationUpdate()
            .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
            .AssertReply("Hello Carlos, nice to talk to you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_SwitchCondition_Number()
        {
            await BuildTestFlow("SwitchCondition.main.dialog")
            .Send("Hi")
            .AssertReply("Age is 22!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_TextInputWithoutProperty()
        {
            await BuildTestFlow("TextInput.WithoutProperty.main.dialog")
            .SendConversationUpdate()
                .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
                .AssertReply("Hello, nice to talk to you!")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_TextInput()
        {
            await BuildTestFlow("TextInput.main.dialog")
            .SendConversationUpdate()
                .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Cancel")
                .AssertReply("Cancel")
                .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos  ") // outputFormat = trim
                .AssertReply("Hello Carlos, nice to talk to you!")
                .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Cancel") // allowInterruptions = notRecognized
                .AssertReply("Hello Cancel, nice to talk to you!")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_NumberInput()
        {
            await BuildTestFlow("NumberInput.main.dialog")
            .SendConversationUpdate()
                .AssertReply("What is your age?")
            .Send("Blablabla")
                .AssertReply("Please input a number.")
            .Send("4")
                .AssertReply("Hello, your age is 4!")
                .AssertReply("2 * 2.2 equals?")
            .Send("4.4")
                .AssertReply("2 * 2.2 equals 4.4, that's right!")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_RepeatDialog()
        {
            await BuildTestFlow("RepeatDialog.main.dialog")
            .SendConversationUpdate()
                .AssertReply("RepeatDialog.main.dialog starting")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you! (type cancel to end this)")
            .Send("hi")
                .AssertReply("RepeatDialog.main.dialog starting")
                .AssertReply("Hello Carlos, nice to meet you! (type cancel to end this)")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_TraceAndLog()
        {
            await BuildTestFlow("TraceAndLog.main.dialog", sendTrace: true)
            .SendConversationUpdate()
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply(activity =>
                {
                    var trace = (Activity)activity;
                    Assert.AreEqual(ActivityTypes.Trace, trace.Type, "should be trace activity");
                    Assert.AreEqual("memory", trace.ValueType, "value type should be memory");
                    Assert.AreEqual("Carlos", ((IDictionary<string, object>)trace.Value)["name"].ToString(), "value should be user object with name='Carlos'");
                })
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_DoActions()
        {
            await BuildTestFlow("DoActions.main.dialog")
            .Send(new Activity(ActivityTypes.ConversationUpdate, membersAdded: new List<ChannelAccount>() { new ChannelAccount("bot", "Bot") }))
            .SendConversationUpdate()
                .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to talk to you!")
                .AssertReply("Hey, I can tell you a joke, or tell your fortune")
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
            await BuildTestFlow("BeginDialog.main.dialog")
            .Send(new Activity(
                ActivityTypes.ConversationUpdate,
                membersAdded: new List<ChannelAccount>() { new ChannelAccount("bot", "Bot") }))
            .SendConversationUpdate()
                .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to talk to you!")
                .AssertReply("Hey, I can tell you a joke, or tell your fortune")
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
            await BuildTestFlow("ChoiceInput.main.dialog")
            .SendConversationUpdate()
                .AssertReply("Please select a value from below:\n\n   1. Test1\n   2. Test2\n   3. Test3")
            .Send("Test1")
                .AssertReply("You select: Test1")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task JsonDialogLoad_ExternalLanguage()
        {
            await BuildTestFlow("ExternalLanguage.main.dialog")
            .SendConversationUpdate()
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
            await BuildTestFlow("ToDoBot.main.dialog")
            .Send(new Activity(ActivityTypes.ConversationUpdate, membersAdded: new List<ChannelAccount>() { new ChannelAccount("bot", "Bot") }))
            .SendConversationUpdate()
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
            await BuildTestFlow("HttpRequest.main.dialog")
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

        private TestFlow BuildTestFlow(string resourceName, bool sendTrace = false)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName), sendTrace);
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .UseResourceExplorer(resourceExplorer)
                .UseAdaptiveDialogs()
                .UseLanguageGeneration(resourceExplorer)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var resource = resourceExplorer.GetResource(resourceName);
            var dialog = DeclarativeTypeLoader.Load<Dialog>(resource, resourceExplorer, DebugSupport.SourceRegistry);
            DialogManager dm = new DialogManager(dialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }
    }
}
