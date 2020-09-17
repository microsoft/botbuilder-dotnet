// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Loader.Tests
{
    public class JsonLoadTests : IClassFixture<ResourceExplorerFixture>
    {
        private static ResourceExplorer _resourceExplorer;
        private static ResourceExplorer _noCycleResourceExplorer;

        public JsonLoadTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorer = resourceExplorerFixture.ResourceExplorer;
            _noCycleResourceExplorer = resourceExplorerFixture.NoCycleResourceExplorer;
        }

        [Fact]
        public async Task JsonDialogLoad_DoubleReference()
        {
            await BuildTestFlow(@"DoubleReference.dialog", nameof(JsonDialogLoad_DoubleReference))
                .SendConversationUpdate()
                .AssertReply("what is your name?")
                .Send("c")
                .AssertReply("sub0")
            .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_CycleDetection()
        {
            await BuildTestFlow(@"Root.dialog", nameof(JsonDialogLoad_CycleDetection))
                .SendConversationUpdate()
                .AssertReply("Hello")
                .Send("Hello what?")
                .AssertReply("World")
                .Send("World what?")
                .AssertReply("Hello")
            .StartTestAsync();
        }

        [Fact]
        public void JsonDialogLoad_CycleDetectionWithNoCycleMode()
        {
            Assert.Throws<InvalidOperationException>(() => BuildNoCycleTestFlow(@"Root.dialog", nameof(JsonDialogLoad_CycleDetectionWithNoCycleMode)));
        }

        [Fact]
        public async Task JsonDialogLoad_Actions()
        {
            await BuildTestFlow(@"Actions.main.dialog", nameof(JsonDialogLoad_Actions))
                .SendConversationUpdate()
                .AssertReply("Action 1")
                .AssertReply("Action 2")
                .AssertReply("Action 3")
            .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_EndTurn()
        {
            await BuildTestFlow("EndTurn.main.dialog", nameof(JsonDialogLoad_EndTurn))
            .Send("hello")
                .AssertReply("What's up?")
            .Send("Nothing")
                .AssertReply("Oh I see!")
            .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_IfProperty()
        {
            await BuildTestFlow("IfCondition.main.dialog", nameof(JsonDialogLoad_IfProperty))
            .SendConversationUpdate()
            .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
            .AssertReply("Hello Carlos, nice to talk to you!")
            .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_SwitchCondition_Number()
        {
            await BuildTestFlow("SwitchCondition.main.dialog", nameof(JsonDialogLoad_SwitchCondition_Number))
            .Send("Hi")
            .AssertReply("Age is 22!")
            .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_TextInputWithoutProperty()
        {
            await BuildTestFlow("TextInput.WithoutProperty.main.dialog", nameof(JsonDialogLoad_TextInputWithoutProperty))
            .SendConversationUpdate()
                .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos")
                .AssertReply("Hello, nice to talk to you!")
                .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_TextInput()
        {
            await BuildTestFlow("TextInput.main.dialog", nameof(JsonDialogLoad_TextInput))
            .SendConversationUpdate()
                .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Cancel")
                .AssertReply("Cancel")
                .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Carlos  ") // outputFormat = trim(this.value)
                .AssertReply("Hello Carlos, nice to talk to you!")
                .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("Cancel") // allowInterruptions = notRecognized
                .AssertReply("Hello Cancel, nice to talk to you!")
                .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_NumberInput()
        {
            await BuildTestFlow("NumberInput.main.dialog", nameof(JsonDialogLoad_NumberInput))
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

        [Fact]
        public async Task JsonDialogLoad_RepeatDialog()
        {
            await BuildTestFlow("RepeatDialog.main.dialog", nameof(JsonDialogLoad_RepeatDialog))
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

        [Fact]
        public async Task JsonDialogLoad_TraceAndLog()
        {
            await BuildTestFlow("TraceAndLog.main.dialog", nameof(JsonDialogLoad_TraceAndLog), true)
            .SendConversationUpdate()
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply(activity =>
                {
                    var trace = (Activity)activity;
                    Assert.Equal("https://www.botframework.com/schemas/botState", trace.ValueType);
                    Assert.Equal(ActivityTypes.Trace, trace.Type);
                })
                .AssertReply(activity =>
                {
                    var trace = (Activity)activity;
                    Assert.Equal(ActivityTypes.Trace, trace.Type);
                    Assert.Equal("memory", trace.ValueType);
                    Assert.Equal("Carlos", (string)((dynamic)trace.Value)["name"]);
                })
            .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_DoActions()
        {
            await BuildTestFlow("DoActions.main.dialog", nameof(JsonDialogLoad_DoActions))
            .Send(new Activity(ActivityTypes.ConversationUpdate, membersAdded: new List<ChannelAccount> { new ChannelAccount("bot", "Bot") }))
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

        [Fact]
        public async Task JsonDialogLoad_BeginDialog()
        {
            await BuildTestFlow("BeginDialog.main.dialog", nameof(JsonDialogLoad_BeginDialog))
            .Send(new Activity(
                ActivityTypes.ConversationUpdate,
                membersAdded: new List<ChannelAccount> { new ChannelAccount("bot", "Bot") }))
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

        [Fact]
        public async Task JsonDialogLoad_ChoiceInputDialog()
        {
            await BuildTestFlow("ChoiceInput.main.dialog", nameof(JsonDialogLoad_ChoiceInputDialog))
            .SendConversationUpdate()
                .AssertReply("Please select a value from below:\n\n   1. Test1\n   2. Test2\n   3. Test3")
            .Send("Test1")
                .AssertReply("You select: Test1")
            .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_ExternalLanguage()
        {
            await BuildTestFlow("ExternalLanguage.main.dialog", nameof(JsonDialogLoad_ExternalLanguage))
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

        [Fact]
        public async Task JsonDialogLoad_ToDoBot()
        {
            await BuildTestFlow("ToDoBot.main.dialog", nameof(JsonDialogLoad_ToDoBot))
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
                .AssertReplyOneOf(new string[]
                {
                    "Your most recent 3 tasks are\n* first\n* second\n* third",
                    "Your most recent 3 tasks are\r\n* first\n* second\n* third",
                })
            .Send("delete todo named second")
                .AssertReply("Successfully removed a todo named \"second\"")
            .Send("show todos")
                .AssertReplyOneOf(new string[]
                {
                    "Your most recent 2 tasks are\r\n* first\n* third",
                    "Your most recent 2 tasks are\n* first\n* third",
                })
            .Send("add a todo")
                .AssertReply("OK, please enter the title of your todo.")
            .Send("cancel")
                .AssertReply("ok.")
            .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_HttpRequest()
        {
            await BuildTestFlow("HttpRequest.main.dialog", nameof(JsonDialogLoad_HttpRequest))
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

        [Fact]
        public async Task JsonDialogLoad_QnAMakerDialog_ActiveLearning_WithProperResponse()
        {
            var suggestionList = new List<string> { "Q1", "Q2", "Q3" };
            var suggestionActivity = QnACardBuilder.GetSuggestionsCard(suggestionList, "Did you mean:", "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await BuildQnAMakerTestFlow(nameof(JsonDialogLoad_QnAMakerDialog_ActiveLearning_WithProperResponse))
            .Send("Q11")
                .AssertReply(suggestionActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("Q1")
                .AssertReply("A1")
            .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_QnAMakerDialog_ActiveLearning_WithNoResponse()
        {
            var suggestionList = new List<string> { "Q1", "Q2", "Q3" };
            var suggestionActivity = QnACardBuilder.GetSuggestionsCard(suggestionList, "Did you mean:", "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();
            const string noAnswerActivity = "Answers not found in kb.";

            await BuildQnAMakerTestFlow(nameof(JsonDialogLoad_QnAMakerDialog_ActiveLearning_WithNoResponse))
            .Send("Q11")
                .AssertReply(suggestionActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("Q12")
                .AssertReply(noAnswerActivity)
            .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_QnAMakerDialog_ActiveLearning_WithNoneOfAboveQuery()
        {
            var suggestionList = new List<string> { "Q1", "Q2", "Q3" };
            var suggestionActivity = QnACardBuilder.GetSuggestionsCard(suggestionList, "Did you mean:", "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await BuildQnAMakerTestFlow(nameof(JsonDialogLoad_QnAMakerDialog_ActiveLearning_WithNoneOfAboveQuery))
            .Send("Q11")
                .AssertReply(suggestionActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("None of the above.")
                .AssertReply("Thanks for the feedback.")
            .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_QnAMakerDialog_IsTest_True()
        {
            await BuildQnAMakerTestFlow_IsTest_True(nameof(JsonDialogLoad_QnAMakerDialog_IsTest_True))
            .Send("Surface book 2 price")
                .AssertReply("Surface book 2 price is $1400.")
            .StartTestAsync();
        }

        [Fact]
        public async Task JsonDialogLoad_QnAMakerDialog_RankerType_QuestionOnly()
        {
            await BuildQnAMakerTestFlow_RankerType_QuestionOnly(nameof(JsonDialogLoad_QnAMakerDialog_RankerType_QuestionOnly))
            .Send("What ranker do you want to use?")
                .AssertReply("We are using QuestionOnly ranker.")
            .StartTestAsync();
        }

        private TestFlow BuildQnAMakerTestFlow(string testName)
        {
            var adapter = InitializeAdapter(testName);
            var dialog = _resourceExplorer.LoadType<AdaptiveDialog>("QnAMakerBot.main.dialog");
            var qnaMakerDialog = (QnAMakerDialog)dialog.Triggers[0].Actions[0];

            dialog.Triggers[0].Actions[0] = qnaMakerDialog;

            return GetTestFlow(dialog, adapter);
        }

        private TestFlow BuildQnAMakerTestFlow_IsTest_True(string testName)
        {
            var adapter = InitializeAdapter(testName);
            var dialog = _resourceExplorer.LoadType<AdaptiveDialog>("QnAMakerBot.main.dialog");
            var qnaMakerDialog = (QnAMakerDialog)dialog.Triggers[0].Actions[0];
            qnaMakerDialog.IsTest = true;
            dialog.Triggers[0].Actions[0] = qnaMakerDialog;

            return GetTestFlow(dialog, adapter);
        }

        private TestFlow BuildQnAMakerTestFlow_RankerType_QuestionOnly(string testName)
        {
            var adapter = InitializeAdapter(testName);
            var dialog = _resourceExplorer.LoadType<AdaptiveDialog>("QnAMakerBot.main.dialog");
            var qnAMakerDialog = (QnAMakerDialog)dialog.Triggers[0].Actions[0];
            var qnaMakerDialog = qnAMakerDialog;
            qnaMakerDialog.RankerType = RankerTypes.QuestionOnly;
            dialog.Triggers[0].Actions[0] = qnaMakerDialog;

            return GetTestFlow(dialog, adapter);
        }

        private TestAdapter InitializeAdapter(string testName, bool sendTrace = false)
        {
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);
            var adapter = new TestAdapter(TestAdapter.CreateConversation(testName), sendTrace);
            adapter
                .UseStorage(storage)
                .UseBotState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            return adapter;
        }

        private TestFlow GetTestFlow(Dialog dialog, TestAdapter adapter, bool allowCycle = true)
        {
            var dm = new DialogManager(dialog)
                .UseResourceExplorer(allowCycle ? _resourceExplorer : _noCycleResourceExplorer)
                .UseLanguageGeneration();

            dm.InitialTurnState.Add<IQnAMakerClient>(new MockQnAMakerClient());

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken).ConfigureAwait(false);
            });
        }

        private TestFlow BuildTestFlow(string resourceName, string testName, bool sendTrace = false)
        {
            var adapter = InitializeAdapter(testName, sendTrace);
            var dialog = _resourceExplorer.LoadType<Dialog>(resourceName);
            return GetTestFlow(dialog, adapter);
        }

        private TestFlow BuildNoCycleTestFlow(string resourceName, string testName, bool sendTrace = false)
        {
            var adapter = InitializeAdapter(testName, sendTrace);
            var dialog = _noCycleResourceExplorer.LoadType<Dialog>(resourceName);
            return GetTestFlow(dialog, adapter, false);
        }
    }
}
