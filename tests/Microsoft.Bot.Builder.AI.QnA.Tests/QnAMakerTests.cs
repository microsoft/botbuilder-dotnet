// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1201 // Elements should appear in the correct order

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.AI.QnA.Tests;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.AI.Tests
{
    [TestClass]
    public class QnAMakerTests
    {
        private const string _knowledgeBaseId = "dummy-id";
        private const string _endpointKey = "dummy-key";
        private const string _hostname = "https://dummy-hostname.azurewebsites.net/qnamaker";

        public TestContext TestContext { get; set; }

        public AdaptiveDialog QnAMakerAction_ActiveLearningDialogBase()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl()).WithContent("{\"question\":\"Q11\",\"top\":3,\"strictFilters\":[],\"metadataBoost\":[],\"scoreThreshold\":0.3,\"context\":{\"previousQnAId\":0,\"previousUserQuery\":\"\"},\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\"}")
                .Respond("application/json", GetResponse("QnaMaker_TopNAnswer.json"));
            mockHttp.When(HttpMethod.Post, GetTrainRequestUrl())
                .Respond(HttpStatusCode.NoContent, "application/json", "{ }");
            mockHttp.When(HttpMethod.Post, GetRequestUrl()).WithContent("{\"question\":\"Q12\",\"top\":3,\"strictFilters\":[],\"metadataBoost\":[],\"scoreThreshold\":0.3,\"context\":{\"previousQnAId\":0,\"previousUserQuery\":\"\"},\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\"}")
               .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer_WhenNoAnswerFoundInKb.json"));

            return CreateQnAMakerActionDialog(mockHttp);
        }

        [TestMethod]
        public async Task QnAMakerAction_ActiveLearningDialog_WithProperResponse()
        {
            var rootDialog = QnAMakerAction_ActiveLearningDialogBase();

            var suggestionList = new List<string> { "Q1", "Q2", "Q3" };
            var suggestionActivity = QnACardBuilder.GetSuggestionsCard(suggestionList, "Did you mean:", "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog)
            .Send("Q11")
                .AssertReply(suggestionActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("Q1")
                .AssertReply("A1")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task QnAMakerAction_ActiveLearningDialog_WithNoResponse()
        {
            var rootDialog = QnAMakerAction_ActiveLearningDialogBase();

            var noAnswerActivity = "No match found, please as another question.";

            var suggestionList = new List<string> { "Q1", "Q2", "Q3" };
            var suggestionActivity = QnACardBuilder.GetSuggestionsCard(suggestionList, "Did you mean:", "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog)
            .Send("Q11")
                .AssertReply(suggestionActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("Q12")
                .AssertReply(noAnswerActivity)
            .StartTestAsync();
        }

        [TestMethod]
        public async Task QnAMakerAction_ActiveLearningDialog_WithNoneOfAboveQuery()
        {
            var rootDialog = QnAMakerAction_ActiveLearningDialogBase();

            var suggestionList = new List<string> { "Q1", "Q2", "Q3" };
            var suggestionActivity = QnACardBuilder.GetSuggestionsCard(suggestionList, "Did you mean:", "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog)
            .Send("Q11")
                .AssertReply(suggestionActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("None of the above.")
                .AssertReply("Thanks for the feedback.")
            .StartTestAsync();
        }

        public AdaptiveDialog QnAMakerAction_MultiTurnDialogBase()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl()).WithContent("{\"question\":\"I have issues related to KB\",\"top\":3,\"strictFilters\":[],\"metadataBoost\":[],\"scoreThreshold\":0.3,\"context\":{\"previousQnAId\":0,\"previousUserQuery\":\"\"},\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\"}")
                .Respond("application/json", GetResponse("QnaMaker_ReturnAnswer_withPrompts.json"));
            mockHttp.When(HttpMethod.Post, GetRequestUrl()).WithContent("{\"question\":\"Accidently deleted KB\",\"top\":3,\"strictFilters\":[],\"metadataBoost\":[],\"scoreThreshold\":0.3,\"context\":{\"previousQnAId\":27,\"previousUserQuery\":\"\"},\"qnaId\":1,\"isTest\":false,\"rankerType\":\"Default\"}")
                .Respond("application/json", GetResponse("QnaMaker_ReturnAnswer_MultiTurnLevel1.json"));

            return CreateQnAMakerActionDialog(mockHttp);
        }

        [TestMethod]
        public async Task QnAMakerAction_MultiTurnDialogBase_WithAnswer()
        {
            var rootDialog = QnAMakerAction_MultiTurnDialogBase();

            var response = JsonConvert.DeserializeObject<QueryResults>(File.ReadAllText(GetFilePath("QnaMaker_ReturnAnswer_withPrompts.json")));
            var promptsActivity = QnACardBuilder.GetQnAPromptsCard(response.Answers[0], "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog)
            .Send("I have issues related to KB")
                .AssertReply(promptsActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("Accidently deleted KB")
                .AssertReply("All deletes are permanent, including question and answer pairs, files, URLs, custom questions and answers, knowledge bases, or Azure resources. Make sure you export your knowledge base from the Settings**page before deleting any part of your knowledge base.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task QnAMakerAction_MultiTurnDialogBase_WithNoAnswer()
        {
            var rootDialog = QnAMakerAction_MultiTurnDialogBase();

            var response = JsonConvert.DeserializeObject<QueryResults>(File.ReadAllText(GetFilePath("QnaMaker_ReturnAnswer_withPrompts.json")));
            var promptsActivity = QnACardBuilder.GetQnAPromptsCard(response.Answers[0], "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog)
            .Send("I have issues related to KB")
                .AssertReply(promptsActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("None of the above.")
                .AssertReply("Thanks for the feedback.")
            .StartTestAsync();
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TraceActivity()
        {
            // Mock Qna
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));
            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                },
                new QnAMakerOptions
                {
                    Top = 1,
                });

            // Invoke flow which uses mock
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new TranscriptLoggerMiddleware(transcriptStore));
            string conversationId = null;

            await new TestFlow(adapter, async (context, ct) =>
            {
                // Simulate Qna Lookup
                if (context?.Activity?.Text.CompareTo("how do I clean the stove?") == 0)
                {
                    var results = await qna.GetAnswersAsync(context);
                    Assert.IsNotNull(results);
                    Assert.AreEqual(results.Length, 1, "should get one result");
                    StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
                }

                conversationId = context.Activity.Conversation.Id;
                var typingActivity = new Activity
                {
                    Type = ActivityTypes.Typing,
                    RelatesTo = context.Activity.RelatesTo,
                };
                await context.SendActivityAsync(typingActivity);
                await Task.Delay(500);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("how do I clean the stove?")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:how do I clean the stove?")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Validate Trace Activity created
            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync("test", conversationId);
            Assert.AreEqual(7, pagedResult.Items.Length);
            Assert.AreEqual("how do I clean the stove?", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.IsTrue(pagedResult.Items[1].Type.CompareTo(ActivityTypes.Trace) == 0);
            var traceInfo = ((JObject)((ITraceActivity)pagedResult.Items[1]).Value).ToObject<QnAMakerTraceInfo>();
            Assert.IsNotNull(traceInfo);
            Assert.IsNotNull(pagedResult.Items[2].AsTypingActivity());
            Assert.AreEqual("echo:how do I clean the stove?", pagedResult.Items[3].AsMessageActivity().Text);
            Assert.AreEqual("bar", pagedResult.Items[4].AsMessageActivity().Text);
            Assert.IsNotNull(pagedResult.Items[5].AsTypingActivity());
            Assert.AreEqual("echo:bar", pagedResult.Items[6].AsMessageActivity().Text);
            foreach (var activity in pagedResult.Items)
            {
                Assert.IsTrue(!string.IsNullOrWhiteSpace(activity.Id));
                Assert.IsTrue(activity.Timestamp > default(DateTimeOffset));
            }
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentException))]
        public async Task QnaMaker_TraceActivity_EmptyText()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            // No text
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = string.Empty,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount(),
            };
            var context = new TurnContext(adapter, activity);

            var results = await qna.GetAnswersAsync(context);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentException))]
        public async Task QnaMaker_TraceActivity_NullText()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            // No text
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = null,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount(),
            };
            var context = new TurnContext(adapter, activity);

            var results = await qna.GetAnswersAsync(context);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QnaMaker_TraceActivity_NullContext()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            var results = await qna.GetAnswersAsync(null);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentException))]
        public async Task QnaMaker_TraceActivity_BadMessage()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            // No text
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            var activity = new Activity
            {
                Type = ActivityTypes.Trace,
                Text = "My Text",
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount(),
            };
            var context = new TurnContext(adapter, activity);

            var results = await qna.GetAnswersAsync(context);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QnaMaker_TraceActivity_NullActivity()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            // No text
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            var context = new MyTurnContext(adapter, null);

            var results = await qna.GetAnswersAsync(context);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswer()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                },
                new QnAMakerOptions
                {
                    Top = 1,
                });

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswerRaw()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var options = new QnAMakerOptions
            {
                Top = 1,
            };

            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                },
                options);

            var results = await qna.GetAnswersRawAsync(GetContext("how do I clean the stove?"), options);
            Assert.IsNotNull(results.Answers);
            Assert.IsTrue(results.ActiveLearningEnabled);
            Assert.AreEqual(results.Answers.Length, 1, "should get one result");
            StringAssert.StartsWith(results.Answers[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_LowScoreVariation()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_TopNAnswer.json"));

            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                },
                new QnAMakerOptions
                {
                    Top = 5,
                });

            var results = await qna.GetAnswersAsync(GetContext("Q11"));
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 4, "should get four results");

            var filteredResults = qna.GetLowScoreVariation(results);
            Assert.IsNotNull(filteredResults);
            Assert.AreEqual(filteredResults.Length, 3, "should get three results");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_CallTrain()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetTrainRequestUrl())
                .Respond(HttpStatusCode.NoContent, "application/json", "{ }");

            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                });

            var feedbackRecords = new FeedbackRecords();

            var feedback1 = new FeedbackRecord
            {
                QnaId = 1,
                UserId = "test",
                UserQuestion = "How are you?",
            };

            var feedback2 = new FeedbackRecord
            {
                QnaId = 2,
                UserId = "test",
                UserQuestion = "What up??",
            };

            feedbackRecords.Records = new FeedbackRecord[] { feedback1, feedback2 };

            await qna.CallTrainAsync(feedbackRecords);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswer_Configuration()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var service = new QnAMakerService
            {
                KbId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Hostname = _hostname,
            };

            var options = new QnAMakerOptions
            {
                Top = 1,
            };

            var client = new HttpClient(mockHttp);
            var qna = new QnAMaker(service, options, client);

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswerWithFiltering()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_UsesStrictFilters_ToReturnAnswer.json"));

            var interceptHttp = new InterceptRequestHandler(mockHttp);

            var qna = GetQnAMaker(
                interceptHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                });

            var options = new QnAMakerOptions
            {
                StrictFilters = new Metadata[]
                {
                    new Metadata() { Name = "topic", Value = "value" },
                },
                Top = 1,
            };

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"), options);
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
            Assert.AreEqual("topic", results[0].Metadata[0].Name);
            Assert.AreEqual("value", results[0].Metadata[0].Value);

            // verify we are actually passing on the options
            var obj = JObject.Parse(interceptHttp.Content);
            Assert.AreEqual(1, obj["top"].Value<int>());
            Assert.AreEqual("topic", obj["strictFilters"][0]["name"].Value<string>());
            Assert.AreEqual("value", obj["strictFilters"][0]["value"].Value<string>());
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_SetScoreThresholdWhenThresholdIsZero()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var qnaWithZeroValueThreshold = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                },
                new QnAMakerOptions()
                {
                    ScoreThreshold = 0.0F,
                });

            var results = await qnaWithZeroValueThreshold
                .GetAnswersAsync(GetContext("how do I clean the stove?"), new QnAMakerOptions() { Top = 1 });

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Length);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TestThreshold()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_TestThreshold.json"));

            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                },
                new QnAMakerOptions
                {
                    Top = 1,
                    ScoreThreshold = 0.99F,
                });

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 0, "should get zero result because threshold");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void QnaMaker_Test_ScoreThresholdTooLarge_OutOfRange()
        {
            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Host = _hostname,
            };

            var tooLargeThreshold = new QnAMakerOptions
            {
                ScoreThreshold = 1.1F,
                Top = 1,
            };

            var qnaWithLargeThreshold = new QnAMaker(endpoint, tooLargeThreshold);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void QnaMaker_Test_ScoreThresholdTooSmall_OutOfRange()
        {
            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Host = _hostname,
            };

            var tooSmallThreshold = new QnAMakerOptions
            {
                ScoreThreshold = -9000.0F,
                Top = 1,
            };

            var qnaWithSmallThreshold = new QnAMaker(endpoint, tooSmallThreshold);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswerWithContext()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswerWithContext.json"));

            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                });

            var options = new QnAMakerOptions()
            {
                Top = 1,
                Context = new QnARequestContext()
                {
                    PreviousQnAId = 5,
                    PreviousUserQuery = "how do I clean the stove?",
                },
            };

            var results = await qna.GetAnswersAsync(GetContext("Where can I buy?"), options);
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Length, "should get one result");
            Assert.AreEqual(55, results[0].Id, "should get context based follow-up");
            Assert.AreEqual(1, results[0].Score, "Score should be high");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswerWithoutContext()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswerWithoutContext.json"));

            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                });

            var options = new QnAMakerOptions()
            {
                Top = 3,
            };

            var results = await qna.GetAnswersAsync(GetContext("Where can I buy?"), options);
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Length, "should get two result");
            Assert.AreNotEqual(1, results[0].Score, "Score should be low");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsHighScoreWhenIdPassed()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswerWithContext.json"));

            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                });

            var options = new QnAMakerOptions()
            {
                Top = 1,
                QnAId = 55,
            };

            var results = await qna.GetAnswersAsync(GetContext("Where can I buy?"), options);
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Length, "should get one result");
            Assert.AreEqual(55, results[0].Id, "should get context based follow-up");
            Assert.AreEqual(1, results[0].Score, "Score should be high");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void QnaMaker_Test_Top_OutOfRange()
        {
            var qna = new QnAMaker(
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                },
                new QnAMakerOptions
                {
                    Top = -1,
                    ScoreThreshold = 0.5F,
                });
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentException))]
        public void QnaMaker_Test_Endpoint_EmptyKbId()
        {
            var qnaNullEndpoint = new QnAMaker(
                new QnAMakerEndpoint()
                {
                    KnowledgeBaseId = string.Empty,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                });
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentException))]
        public void QnaMaker_Test_Endpoint_EmptyEndpointKey()
        {
            var qnaNullEndpoint = new QnAMaker(
                new QnAMakerEndpoint()
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = string.Empty,
                    Host = _hostname,
                });
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentException))]
        public void QnaMaker_Test_Endpoint_EmptyHost()
        {
            var qnaNullEndpoint = new QnAMaker(
                new QnAMakerEndpoint()
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = string.Empty,
                });
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_UserAgent()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var interceptHttp = new InterceptRequestHandler(mockHttp);

            var qna = GetQnAMaker(
                interceptHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                },
                new QnAMakerOptions
                {
                    Top = 1,
                });

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));

            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");

            // Verify that we added the bot.builder package details.
            Assert.IsTrue(interceptHttp.UserAgent.Contains("Microsoft.Bot.Builder.AI.QnA/4"));
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(NotSupportedException))]
        public async Task QnaMaker_V2LegacyEndpoint_ConvertsToHaveIdPropertyInResult()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetV2LegacyRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_LegacyEndpointAnswer.json"));

            var v2LegacyEndpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Host = $"{_hostname}/v2.0"
            };

            var v2Qna = GetQnAMaker(mockHttp, v2LegacyEndpoint);

            var v2legacyResult = await v2Qna.GetAnswersAsync(GetContext("How do I be the best?"));

            Assert.IsNotNull(v2legacyResult);
            Assert.IsTrue(v2legacyResult[0]?.Id != null);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(NotSupportedException))]
        public async Task QnaMaker_V3LegacyEndpoint_ConvertsToHaveIdPropertyInResult()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetV3LegacyRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_LegacyEndpointAnswer.json"));

            var v3LegacyEndpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Host = $"{_hostname}/v3.0"
            };

            var v3Qna = GetQnAMaker(mockHttp, v3LegacyEndpoint);

            var v3legacyResult = await v3Qna.GetAnswersAsync(GetContext("How do I be the best?"));

            Assert.IsNotNull(v3legacyResult);
            Assert.IsTrue(v3legacyResult[0]?.Id != null);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswerWithMetadataBoost()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswersWithMetadataBoost.json"));

            var interceptHttp = new InterceptRequestHandler(mockHttp);

            var qna = GetQnAMaker(
                interceptHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                });

            var options = new QnAMakerOptions
            {
                MetadataBoost = new Metadata[]
                {
                    new Metadata() { Name = "artist", Value = "drake" },
                },
                Top = 1,
            };

            var results = await qna.GetAnswersAsync(GetContext("who loves me?"), options);

            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "Kiki");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TestThresholdInQueryOption()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer_GivenScoreThresholdQueryOption.json"));

            var interceptHttp = new InterceptRequestHandler(mockHttp);

            var qna = GetQnAMaker(
                interceptHttp,
                new QnAMakerEndpoint()
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                });

            var queryOptionsWithScoreThreshold = new QnAMakerOptions()
            {
                ScoreThreshold = 0.5F,
                Top = 2,
            };

            var result = await qna.GetAnswersAsync(
                    GetContext("What happens when you hug a porcupine?"),
                    queryOptionsWithScoreThreshold);

            Assert.IsNotNull(result);

            var obj = JObject.Parse(interceptHttp.Content);
            Assert.AreEqual(2, obj["top"].Value<int>());
            Assert.AreEqual(0.5F, obj["scoreThreshold"].Value<float>());
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task QnaMaker_Test_UnsuccessfulResponse()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond(System.Net.HttpStatusCode.BadGateway);

            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint()
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                });

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_IsTest_True()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_IsTest_True.json"));

            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                });

            var qnaamkerOptions = new QnAMakerOptions
            {
                Top = 1,
                IsTest = true
            };

            var results = await qna.GetAnswersAsync(GetContext("Q11"), qnaamkerOptions);
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 0, "should get no results");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_RankerType_QuestionOnly()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_RankerType_QuestionOnly.json"));

            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                });

            var qnamakerOptions = new QnAMakerOptions
            {
                Top = 1,
                RankerType = "QuestionOnly"
            };

            var results = await qna.GetAnswersAsync(GetContext("Q11"), qnamakerOptions);
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 2, "should get two results");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_Test_Options_Hydration()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var interceptHttp = new InterceptRequestHandler(mockHttp);

            var noFiltersOptions = new QnAMakerOptions()
            {
                Top = 30,
            };

            var qna = GetQnAMaker(
                interceptHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                },
                noFiltersOptions);

            var oneFilteredOption = new QnAMakerOptions()
            {
                Top = 30,
                StrictFilters = new Metadata[]
                {
                    new Metadata()
                    {
                        Name = "movie",
                        Value = "disney",
                    },
                },
            };

            var twoStrictFiltersOptions = new QnAMakerOptions()
            {
                Top = 30,
                StrictFilters = new Metadata[]
                {
                    new Metadata()
                    {
                        Name = "movie",
                        Value = "disney",
                    },
                    new Metadata()
                    {
                        Name = "home",
                        Value = "floating",
                    },
                },
            };
            var allChangedRequestOptions = new QnAMakerOptions()
            {
                Top = 2000,
                ScoreThreshold = 0.42F,
                StrictFilters = new Metadata[]
                {
                    new Metadata()
                    {
                        Name = "dog",
                        Value = "samoyed",
                    },
                },
            };

            var context = GetContext("up");

            // Ensure that options from previous requests do not bleed over to the next,
            // And that the options set in the constructor are not overwritten improperly by options passed into .GetAnswersAsync()
            var noFilterResults1 = await qna.GetAnswersAsync(context, noFiltersOptions);
            var requestContent1 = JsonConvert.DeserializeObject<CapturedRequest>(interceptHttp.Content);

            var twoFiltersResults = await qna.GetAnswersAsync(context, twoStrictFiltersOptions);
            var requestContent2 = JsonConvert.DeserializeObject<CapturedRequest>(interceptHttp.Content);

            var oneFilterResults = await qna.GetAnswersAsync(context, oneFilteredOption);
            var requestContent3 = JsonConvert.DeserializeObject<CapturedRequest>(interceptHttp.Content);

            var noFilterResults2 = await qna.GetAnswersAsync(context);
            var requestContent4 = JsonConvert.DeserializeObject<CapturedRequest>(interceptHttp.Content);

            var allChangedOptionsResult = await qna.GetAnswersAsync(context, allChangedRequestOptions);
            var requestContent5 = JsonConvert.DeserializeObject<CapturedRequest>(interceptHttp.Content);

            var noOptionsResults = await qna.GetAnswersAsync(context);
            var requestContent6 = JsonConvert.DeserializeObject<CapturedRequest>(interceptHttp.Content);

            Assert.AreEqual(0, requestContent1.StrictFilters.Length);
            Assert.AreEqual(2, requestContent2.StrictFilters.Length);
            Assert.AreEqual(1, requestContent3.StrictFilters.Length);
            Assert.AreEqual(0, requestContent4.StrictFilters.Length);

            Assert.AreEqual(2000, requestContent5.Top);
            Assert.AreEqual(0.42, Math.Round(requestContent5.ScoreThreshold, 2));
            Assert.AreEqual(1, requestContent5.StrictFilters.Length);

            Assert.AreEqual(30, requestContent6.Top);
            Assert.AreEqual(0.3, Math.Round(requestContent6.ScoreThreshold, 2));
            Assert.AreEqual(0, requestContent6.StrictFilters.Length);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [TestCategory("Telemetry")]
        public async Task Telemetry_NullTelemetryClient()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var client = new HttpClient(mockHttp);

            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Host = _hostname,
            };
            var options = new QnAMakerOptions
            {
                Top = 1,
            };

            // Act (Null Telemetry client)
            //    This will default to the NullTelemetryClient which no-ops all calls.
            var qna = new QnAMaker(endpoint, options, client, null, true);
            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));

            // Assert - Validate we didn't break QnA functionality.
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
            StringAssert.StartsWith(results[0].Source, "Editorial");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [TestCategory("Telemetry")]
        public async Task Telemetry_ReturnsAnswer()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var client = new HttpClient(mockHttp);

            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Host = _hostname,
            };
            var options = new QnAMakerOptions
            {
                Top = 1,
            };
            var telemetryClient = new Mock<IBotTelemetryClient>();

            // Act - See if we get data back in telemetry
            var qna = new QnAMaker(endpoint, options, client, telemetryClient: telemetryClient.Object, logPersonalInformation: true);
            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));

            // Assert - Check Telemetry logged
            Assert.AreEqual(telemetryClient.Invocations.Count, 1);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments.Count, 3);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("knowledgeBaseId"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("matchedQuestion"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("question"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("questionId"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("answer"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["answer"], "BaseCamp: You can use a damp rag to clean around the Power Pack");
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("articleFound"));
            Assert.AreEqual(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).Count, 1);
            Assert.IsTrue(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("score"));

            // Assert - Validate we didn't break QnA functionality.
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
            StringAssert.StartsWith(results[0].Source, "Editorial");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [TestCategory("Telemetry")]
        public async Task Telemetry_ReturnsAnswer_WhenNoAnswerFoundInKB()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer_WhenNoAnswerFoundInKb.json"));

            var client = new HttpClient(mockHttp);

            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Host = _hostname,
            };
            var options = new QnAMakerOptions
            {
                Top = 1,
            };
            var telemetryClient = new Mock<IBotTelemetryClient>();

            // Act - See if we get data back in telemetry
            var qna = new QnAMaker(endpoint, options, client, telemetryClient: telemetryClient.Object, logPersonalInformation: true);
            var results = await qna.GetAnswersAsync(GetContext("what is the answer to my nonsense question?"));

            // Assert - Check Telemetry logged
            Assert.AreEqual(telemetryClient.Invocations.Count, 1);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments.Count, 3);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("knowledgeBaseId"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("matchedQuestion"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["matchedQuestion"], "No Qna Question matched");
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("question"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("questionId"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("answer"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["answer"], "No Qna Answer matched");
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("articleFound"));
            Assert.AreEqual(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).Count, 0);

            // Assert - Validate we didn't break QnA functionality.
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Length);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [TestCategory("Telemetry")]
        public async Task Telemetry_PII()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var client = new HttpClient(mockHttp);

            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Host = _hostname,
            };
            var options = new QnAMakerOptions
            {
                Top = 1,
            };
            var telemetryClient = new Mock<IBotTelemetryClient>();

            // Act
            var qna = new QnAMaker(endpoint, options, client, telemetryClient.Object, false);
            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));

            // Assert - Validate PII properties not logged.
            Assert.AreEqual(telemetryClient.Invocations.Count, 1);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments.Count, 3);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("knowledgeBaseId"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("matchedQuestion"));
            Assert.IsFalse(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("question"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("questionId"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("answer"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["answer"], "BaseCamp: You can use a damp rag to clean around the Power Pack");
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("articleFound"));
            Assert.AreEqual(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).Count, 1);
            Assert.IsTrue(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("score"));

            // Assert - Validate we didn't break QnA functionality.
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
            StringAssert.StartsWith(results[0].Source, "Editorial");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [TestCategory("Telemetry")]
        public async Task Telemetry_Override()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var client = new HttpClient(mockHttp);

            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Host = _hostname,
            };
            var options = new QnAMakerOptions
            {
                Top = 1,
            };
            var telemetryClient = new Mock<IBotTelemetryClient>();

            // Act - Override the QnaMaker object to log custom stuff and honor parms passed in.
            var telemetryProperties = new Dictionary<string, string>
            {
                { "Id", "MyID" },
            };
            var qna = new OverrideTelemetry(endpoint, options, client, telemetryClient.Object, false);
            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"), null, telemetryProperties);

            // Assert
            Assert.AreEqual(telemetryClient.Invocations.Count, 2);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments.Count, 3);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).Count == 2);
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("MyImportantProperty"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["MyImportantProperty"], "myImportantValue");
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("Id"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["Id"], "MyID");

            Assert.AreEqual(telemetryClient.Invocations[1].Arguments[0], "MySecondEvent");
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[1].Arguments[1]).ContainsKey("MyImportantProperty2"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[1].Arguments[1])["MyImportantProperty2"], "myImportantValue2");

            // Validate we didn't break QnA functionality.
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
            StringAssert.StartsWith(results[0].Source, "Editorial");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [TestCategory("Telemetry")]
        public async Task Telemetry_AdditionalPropsMetrics()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var client = new HttpClient(mockHttp);

            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Host = _hostname,
            };
            var options = new QnAMakerOptions
            {
                Top = 1,
            };
            var telemetryClient = new Mock<IBotTelemetryClient>();

            // Act - Pass in properties during QnA invocation
            var qna = new QnAMaker(endpoint, options, client, telemetryClient.Object, false);
            var telemetryProperties = new Dictionary<string, string>
            {
                { "MyImportantProperty", "myImportantValue" },
            };
            var telemetryMetrics = new Dictionary<string, double>
            {
                { "MyImportantMetric", 3.14159 },
            };

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"), null, telemetryProperties, telemetryMetrics);

            // Assert - added properties were added.
            Assert.AreEqual(telemetryClient.Invocations.Count, 1);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments.Count, 3);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey(QnATelemetryConstants.KnowledgeBaseIdProperty));
            Assert.IsFalse(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey(QnATelemetryConstants.QuestionProperty));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey(QnATelemetryConstants.MatchedQuestionProperty));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey(QnATelemetryConstants.QuestionIdProperty));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey(QnATelemetryConstants.AnswerProperty));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["answer"], "BaseCamp: You can use a damp rag to clean around the Power Pack");
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("MyImportantProperty"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["MyImportantProperty"], "myImportantValue");

            Assert.AreEqual(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).Count, 2);
            Assert.IsTrue(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("score"));
            Assert.IsTrue(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("MyImportantMetric"));
            Assert.AreEqual(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2])["MyImportantMetric"], 3.14159);

            // Validate we didn't break QnA functionality.
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
            StringAssert.StartsWith(results[0].Source, "Editorial");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [TestCategory("Telemetry")]
        public async Task Telemetry_AdditionalPropsOverride()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var client = new HttpClient(mockHttp);

            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Host = _hostname,
            };
            var options = new QnAMakerOptions
            {
                Top = 1,
            };
            var telemetryClient = new Mock<IBotTelemetryClient>();

            // Act - Pass in properties during QnA invocation that override default properties
            //  NOTE: We are invoking this with PII turned OFF, and passing a PII property (originalQuestion).
            var qna = new QnAMaker(endpoint, options, client, telemetryClient.Object, false);
            var telemetryProperties = new Dictionary<string, string>
            {
                { "knowledgeBaseId", "myImportantValue" },
                { "originalQuestion", "myImportantValue2" },
            };
            var telemetryMetrics = new Dictionary<string, double>
            {
                { "score", 3.14159 },
            };

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"), null, telemetryProperties, telemetryMetrics);

            // Assert - added properties were added.
            Assert.AreEqual(telemetryClient.Invocations.Count, 1);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments.Count, 3);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("knowledgeBaseId"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["knowledgeBaseId"], "myImportantValue");
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("matchedQuestion"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["originalQuestion"], "myImportantValue2");
            Assert.IsFalse(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("question"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("questionId"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("answer"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["answer"], "BaseCamp: You can use a damp rag to clean around the Power Pack");
            Assert.IsFalse(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("MyImportantProperty"));

            Assert.AreEqual(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).Count, 1);
            Assert.IsTrue(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("score"));
            Assert.AreEqual(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2])["score"], 3.14159);
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [TestCategory("Telemetry")]
        public async Task Telemetry_FillPropsOverride()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var client = new HttpClient(mockHttp);

            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Host = _hostname,
            };
            var options = new QnAMakerOptions
            {
                Top = 1,
            };
            var telemetryClient = new Mock<IBotTelemetryClient>();

            // Act - Pass in properties during QnA invocation that override default properties
            //       In addition Override with derivation.  This presents an interesting question of order of setting properties.
            //       If I want to override "originalQuestion" property:
            //           - Set in "Stock" schema
            //           - Set in derived QnAMaker class
            //           - Set in GetAnswersAsync
            //       Logically, the GetAnswersAync should win.  But ultimately OnQnaResultsAsync decides since it is the last
            //       code to touch the properties before logging (since it actually logs the event).
            var qna = new OverrideFillTelemetry(endpoint, options, client, telemetryClient.Object, false);
            var telemetryProperties = new Dictionary<string, string>
            {
                { "knowledgeBaseId", "myImportantValue" },
                { "matchedQuestion", "myImportantValue2" },
            };
            var telemetryMetrics = new Dictionary<string, double>
            {
                { "score", 3.14159 },
            };

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"), null, telemetryProperties, telemetryMetrics);

            // Assert - added properties were added.
            Assert.AreEqual(telemetryClient.Invocations.Count, 2);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments.Count, 3);
            Assert.AreEqual(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).Count, 6);
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("knowledgeBaseId"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["knowledgeBaseId"], "myImportantValue");
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("matchedQuestion"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["matchedQuestion"], "myImportantValue2");
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("questionId"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("answer"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["answer"], "BaseCamp: You can use a damp rag to clean around the Power Pack");
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("articleFound"));
            Assert.IsTrue(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("MyImportantProperty"));
            Assert.AreEqual(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["MyImportantProperty"], "myImportantValue");

            Assert.AreEqual(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).Count, 1);
            Assert.IsTrue(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("score"));
            Assert.AreEqual(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2])["score"], 3.14159);
        }

        private static TurnContext GetContext(string utterance)
        {
            var b = new TestAdapter();
            var a = new Activity
            {
                Type = ActivityTypes.Message,
                Text = utterance,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount(),
            };
            return new TurnContext(b, a);
        }

        private TestFlow CreateFlow(Dialog rootDialog)
        {
            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(storage)
                .UseState(userState, conversationState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            DialogManager dm = new DialogManager(rootDialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }

        public class QnaMakerTestDialog : ComponentDialog, IDialogDependencies
        {
            public QnaMakerTestDialog(string knowledgeBaseId, string endpointKey, string hostName, HttpClient httpClient)
                : base(nameof(QnaMakerTestDialog))
            {
                AddDialog(new QnAMakerDialog(knowledgeBaseId, endpointKey, hostName, httpClient: httpClient));
            }

            public override Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default)
            {
                return this.ContinueDialogAsync(outerDc, cancellationToken);
            }

            public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
            {
                if (dc.Context.Activity.Text == "moo")
                {
                    await dc.Context.SendActivityAsync("Yippee ki-yay!");
                    return Dialog.EndOfTurn;
                }
                else
                {
                    return await dc.BeginDialogAsync("qnaDialog");
                }
            }

            public IEnumerable<Dialog> GetDependencies()
            {
                return Dialogs.GetDialogs();
            }

            public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
            {
                if ((bool)result == false)
                {
                    await dc.Context.SendActivityAsync("I didn't understand that.");
                }

                return await base.ResumeDialogAsync(dc, reason, result, cancellationToken);
            }
        }

        private AdaptiveDialog CreateQnAMakerActionDialog(MockHttpMessageHandler mockHttp)
        {
            var client = new HttpClient(mockHttp);

            var noAnswerActivity = new ActivityTemplate("No match found, please as another question.");
            var host = "https://dummy-hostname.azurewebsites.net/qnamaker";
            var knowlegeBaseId = "dummy-id";
            var endpointKey = "dummy-key";
            var activeLearningCardTitle = "QnAMaker Active Learning";

            var outerDialog = new AdaptiveDialog("outer")
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new QnAMakerDialog()
                            {
                                KnowledgeBaseId = knowlegeBaseId,
                                HostName = host,
                                EndpointKey = endpointKey,
                                HttpClient = client,
                                NoAnswer = noAnswerActivity,
                                ActiveLearningCardTitle = activeLearningCardTitle,
                                CardNoMatchText = "None of the above.",
                            }
                        }
                    }
                }
            };

            var rootDialog = new AdaptiveDialog("root")
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog(outerDialog.Id)
                        }
                    },
                    new OnDialogEvent()
                    {
                        Event = "UnhandledUnknownIntent",
                        Actions = new List<Dialog>()
                        {
                            new EditArray(),
                            new SendActivity("magenta")
                        }
                    }
                }
            };
            rootDialog.Dialogs.Add(outerDialog);
            return rootDialog;
        }

        private string GetV2LegacyRequestUrl() => $"{_hostname}/v2.0/knowledgebases/{_knowledgeBaseId}/generateanswer";

        private string GetV3LegacyRequestUrl() => $"{_hostname}/v3.0/knowledgebases/{_knowledgeBaseId}/generateanswer";

        private string GetRequestUrl() => $"{_hostname}/knowledgebases/{_knowledgeBaseId}/generateanswer";

        private string GetTrainRequestUrl() => $"{_hostname}/knowledgebases/{_knowledgeBaseId}/train";

        private Stream GetResponse(string fileName)
        {
            var path = GetFilePath(fileName);
            return File.OpenRead(path);
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(Environment.CurrentDirectory, "TestData", fileName);
        }

        /// <summary>
        /// Return a stock Mocked Qna thats loaded with QnaMaker_ReturnsAnswer.json
        /// Used for tests that just require any old qna instance.
        /// </summary>
        /// <returns>
        /// QnAMaker.
        /// </returns>
        private QnAMaker QnaReturnsAnswer()
        {
            // Mock Qna
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                    .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));
            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                },
                new QnAMakerOptions
                {
                    Top = 1,
                });
            return qna;
        }

        private QnAMaker GetQnAMaker(HttpMessageHandler messageHandler, QnAMakerEndpoint endpoint, QnAMakerOptions options = null)
        {
            var client = new HttpClient(messageHandler);
            return new QnAMaker(endpoint, options, client);
        }

        public class OverrideTelemetry : QnAMaker
        {
            public OverrideTelemetry(QnAMakerEndpoint endpoint, QnAMakerOptions options, HttpClient httpClient, IBotTelemetryClient telemetryClient, bool logPersonalInformation)
                : base(endpoint, options, httpClient, telemetryClient, logPersonalInformation)
            {
            }

            protected override Task OnQnaResultsAsync(
                                        QueryResult[] queryResults,
                                        ITurnContext turnContext,
                                        Dictionary<string, string> telemetryProperties = null,
                                        Dictionary<string, double> telemetryMetrics = null,
                                        CancellationToken cancellationToken = default(CancellationToken))
            {
                var properties = telemetryProperties ?? new Dictionary<string, string>();

                // GetAnswerAsync overrides derived class.
                properties.TryAdd("MyImportantProperty", "myImportantValue");

                // Log event
                TelemetryClient.TrackEvent(
                                QnATelemetryConstants.QnaMsgEvent,
                                properties);

                // Create second event.
                var secondEventProperties = new Dictionary<string, string>();
                secondEventProperties.Add("MyImportantProperty2", "myImportantValue2");
                TelemetryClient.TrackEvent(
                                "MySecondEvent",
                                secondEventProperties);
                return Task.CompletedTask;
            }
        }

        public class OverrideFillTelemetry : QnAMaker
        {
            public OverrideFillTelemetry(QnAMakerEndpoint endpoint, QnAMakerOptions options, HttpClient httpClient, IBotTelemetryClient telemetryClient, bool logPersonalInformation)
                : base(endpoint, options, httpClient, telemetryClient, logPersonalInformation)
            {
            }

            protected override async Task OnQnaResultsAsync(
                                        QueryResult[] queryResults,
                                        ITurnContext turnContext,
                                        Dictionary<string, string> telemetryProperties = null,
                                        Dictionary<string, double> telemetryMetrics = null,
                                        CancellationToken cancellationToken = default(CancellationToken))
            {
                var eventData = await FillQnAEventAsync(queryResults, turnContext, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);

                // Add my property
                eventData.Properties.Add("MyImportantProperty", "myImportantValue");

                // Log QnaMessage event
                TelemetryClient.TrackEvent(
                                QnATelemetryConstants.QnaMsgEvent,
                                eventData.Properties,
                                eventData.Metrics);

                // Create second event.
                var secondEventProperties = new Dictionary<string, string>();
                secondEventProperties.Add("MyImportantProperty2", "myImportantValue2");
                TelemetryClient.TrackEvent(
                                "MySecondEvent",
                                secondEventProperties);
            }
        }

        private class CapturedRequest
        {
            public string[] Questions { get; set; }

            public int Top { get; set; }

            public Metadata[] StrictFilters { get; set; }

            public Metadata[] MetadataBoost { get; set; }

            public float ScoreThreshold { get; set; }
        }
    }
}
