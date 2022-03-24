﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1201 // Elements should appear in the correct order

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
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
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Xunit;

namespace Microsoft.Bot.Builder.AI.Tests
{
    /// <summary>
    /// Defines the <see cref="QnAMakerTests" />.
    /// </summary>
    public class QnAMakerTests
    {
        /// <summary>
        /// Defines the _knowledgeBaseId.
        /// </summary>
        private const string _knowledgeBaseId = "dummy-id";

        /// <summary>
        /// Defines the _endpointKey.
        /// </summary>
        private const string _endpointKey = "dummy-key";

        /// <summary>
        /// Defines the _hostname.
        /// </summary>
        private const string _hostname = "https://dummy-hostname.azurewebsites.net/qnamaker";

        /// <summary>
        /// The QnAMakerAction_ActiveLearningDialogBase.
        /// </summary>
        /// <returns>The <see cref="AdaptiveDialog"/>.</returns>
        public AdaptiveDialog QnAMakerAction_ActiveLearningDialogBase()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetTrainRequestUrl())
                .Respond(HttpStatusCode.NoContent, "application/json", "{ }");
            mockHttp.When(HttpMethod.Post, GetRequestUrl()).WithContent("{\"question\":\"Q12\",\"top\":3,\"strictFilters\":[],\"scoreThreshold\":30.0,\"context\":{\"previousQnAId\":0,\"previousUserQuery\":\"\"},\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\",\"StrictFiltersCompoundOperationType\":0}")
               .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer_WhenNoAnswerFoundInKb.json"));
            mockHttp.When(HttpMethod.Post, GetRequestUrl()).WithContent("{\"question\":\"Q11\",\"top\":3,\"strictFilters\":[],\"scoreThreshold\":30.0,\"context\":{\"previousQnAId\":0,\"previousUserQuery\":\"\"},\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\",\"StrictFiltersCompoundOperationType\":0}")
               .Respond("application/json", GetResponse("QnaMaker_TopNAnswer.json"));
            return CreateQnAMakerActionDialog(mockHttp);
        }

        /// <summary>
        /// The QnAMakerAction_ActiveLearningDialog_WithProperResponse.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        public async Task QnAMakerAction_ActiveLearningDialog_WithProperResponse()
        {
            var rootDialog = QnAMakerAction_ActiveLearningDialogBase();

            var suggestionList = new List<string> { "Q1", "Q2", "Q3" };
            var suggestionActivity = QnACardBuilder.GetSuggestionsCard(suggestionList, "Did you mean:", "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog, "QnAMakerAction_ActiveLearningDialog_WithProperResponse")
            .Send("Q11")
                .AssertReply(suggestionActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("Q1")
                .AssertReply("A1")
            .StartTestAsync();
        }

        /// <summary>
        /// The QnAMakerAction_ActiveLearningDialog_WithNoResponse.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        public async Task QnAMakerAction_ActiveLearningDialog_WithNoResponse()
        {
            var rootDialog = QnAMakerAction_ActiveLearningDialogBase();

            const string noAnswerActivity = "No match found, please ask another question.";

            var suggestionList = new List<string> { "Q1", "Q2", "Q3" };
            var suggestionActivity = QnACardBuilder.GetSuggestionsCard(suggestionList, "Did you mean:", "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog, "QnAMakerAction_ActiveLearningDialog_WithNoResponse")
            .Send("Q11")
                .AssertReply(suggestionActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("Q12")
                .AssertReply(noAnswerActivity)
            .StartTestAsync();
        }

        /// <summary>
        /// The QnAMakerAction_ActiveLearningDialog_WithNoneOfAboveQuery.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        public async Task QnAMakerAction_ActiveLearningDialog_WithNoneOfAboveQuery()
        {
            var rootDialog = QnAMakerAction_ActiveLearningDialogBase();

            var suggestionList = new List<string> { "Q1", "Q2", "Q3" };
            var suggestionActivity = QnACardBuilder.GetSuggestionsCard(suggestionList, "Did you mean:", "None of the above.");
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog, "QnAMakerAction_ActiveLearningDialog_WithNoneOfAboveQuery")
            .Send("Q11")
                .AssertReply(suggestionActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("None of the above.")
                .AssertReply("Thanks for the feedback.")
            .StartTestAsync();
        }

        /// <summary>
        /// The QnAMakerAction_MultiTurnDialogBase.
        /// </summary>
        /// <returns>The <see cref="AdaptiveDialog"/>.</returns>
        public AdaptiveDialog QnAMakerAction_MultiTurnDialogBase()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When(HttpMethod.Post, GetRequestUrl()).WithContent("{\"question\":\"I have issues related to KB\",\"top\":3,\"strictFilters\":[],\"scoreThreshold\":30.0,\"context\":{\"previousQnAId\":0,\"previousUserQuery\":\"\"},\"qnaId\":0,\"isTest\":false,\"rankerType\":\"Default\",\"StrictFiltersCompoundOperationType\":0}")
                .Respond("application/json", GetResponse("QnaMaker_ReturnAnswer_withPrompts.json"));
            mockHttp.When(HttpMethod.Post, GetRequestUrl()).WithContent("{\"question\":\"Accidently deleted KB\",\"top\":3,\"strictFilters\":[],\"scoreThreshold\":30.0,\"context\":{\"previousQnAId\":27,\"previousUserQuery\":\"\"},\"qnaId\":1,\"isTest\":false,\"rankerType\":\"Default\",\"StrictFiltersCompoundOperationType\":0}")
                .Respond("application/json", GetResponse("QnaMaker_ReturnAnswer_MultiTurnLevel1.json"));

            return CreateQnAMakerActionDialog(mockHttp);
        }

        /// <summary>
        /// The QnAMakerAction_MultiTurnDialogBase_WithAnswer.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        public async Task QnAMakerAction_MultiTurnDialogBase_WithAnswer()
        {
            var rootDialog = QnAMakerAction_MultiTurnDialogBase();

            var response = JsonConvert.DeserializeObject<QueryResults>(File.ReadAllText(GetFilePath("QnaMaker_ReturnAnswer_withPrompts.json")));
            var promptsActivity = QnACardBuilder.GetQnAPromptsCard(response.Answers[0]);
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog, nameof(QnAMakerAction_MultiTurnDialogBase_WithAnswer))
            .Send("I have issues related to KB")
                .AssertReply(promptsActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("Accidently deleted KB")
                .AssertReply("All deletes are permanent, including question and answer pairs, files, URLs, custom questions and answers, knowledge bases, or Azure resources. Make sure you export your knowledge base from the Settings**page before deleting any part of your knowledge base.")
            .StartTestAsync();
        }

        /// <summary>
        /// The QnAMakerAction_MultiTurnDialogBase_WithNoAnswer.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        public async Task QnAMakerAction_MultiTurnDialogBase_WithNoAnswer()
        {
            var rootDialog = QnAMakerAction_MultiTurnDialogBase();

            var response = JsonConvert.DeserializeObject<QueryResults>(File.ReadAllText(GetFilePath("QnaMaker_ReturnAnswer_withPrompts.json")));
            var promptsActivity = QnACardBuilder.GetQnAPromptsCard(response.Answers[0]);
            var qnAMakerCardEqualityComparer = new QnAMakerCardEqualityComparer();

            await CreateFlow(rootDialog, nameof(QnAMakerAction_MultiTurnDialogBase_WithNoAnswer))
            .Send("I have issues related to KB")
                .AssertReply(promptsActivity, equalityComparer: qnAMakerCardEqualityComparer)
            .Send("None of the above.")
                .AssertReply("Thanks for the feedback.")
            .StartTestAsync();
        }

        /// <summary>
        /// The QnaMaker_TraceActivity.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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
            var adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(QnaMaker_TraceActivity)))
                .Use(new TranscriptLoggerMiddleware(transcriptStore));
            string conversationId = null;

            await new TestFlow(adapter, async (context, ct) =>
            {
                // Simulate Qna Lookup
                if (context?.Activity?.Text.CompareTo("how do I clean the stove?") == 0)
                {
                    var results = await qna.GetAnswersAsync(context);
                    Assert.NotNull(results);
                    Assert.Single(results);
                    Assert.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack", results[0].Answer);
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
                    .AssertReply((activity) => Assert.Equal(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:how do I clean the stove?")
                .Send("bar")
                    .AssertReply((activity) => Assert.Equal(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Validate Trace Activity created
            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync("test", conversationId);
            Assert.Equal(7, pagedResult.Items.Length);
            Assert.Equal("how do I clean the stove?", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.Equal(0, pagedResult.Items[1].Type.CompareTo(ActivityTypes.Trace));
            var traceInfo = ((JObject)((ITraceActivity)pagedResult.Items[1]).Value).ToObject<QnAMakerTraceInfo>();
            Assert.NotNull(traceInfo);
            Assert.NotNull(pagedResult.Items[2].AsTypingActivity());
            Assert.Equal("echo:how do I clean the stove?", pagedResult.Items[3].AsMessageActivity().Text);
            Assert.Equal("bar", pagedResult.Items[4].AsMessageActivity().Text);
            Assert.NotNull(pagedResult.Items[5].AsTypingActivity());
            Assert.Equal("echo:bar", pagedResult.Items[6].AsMessageActivity().Text);
            foreach (var activity in pagedResult.Items)
            {
                Assert.False(string.IsNullOrWhiteSpace(activity.Id));
                Assert.True(activity.Timestamp > default(DateTimeOffset));
            }
        }

        /// <summary>
        /// The QnaMaker_TraceActivity_EmptyText.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public async Task QnaMaker_TraceActivity_EmptyText()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            // No text
            var adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(QnaMaker_TraceActivity_EmptyText)));
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = string.Empty,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount(),
            };
            var context = new TurnContext(adapter, activity);

            await Assert.ThrowsAsync<ArgumentException>(() => qna.GetAnswersAsync(context));
        }

        /// <summary>
        /// The QnaMaker_TraceActivity_NullText.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public async Task QnaMaker_TraceActivity_NullText()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            // No text
            var adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(QnaMaker_TraceActivity_NullText)));
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = null,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount(),
            };
            var context = new TurnContext(adapter, activity);

            await Assert.ThrowsAsync<ArgumentException>(() => qna.GetAnswersAsync(context));
        }

        /// <summary>
        /// The QnaMaker_TraceActivity_NullContext.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public async Task QnaMaker_TraceActivity_NullContext()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            await Assert.ThrowsAsync<ArgumentNullException>(() => qna.GetAnswersAsync(null));
        }

        /// <summary>
        /// The QnaMaker_TraceActivity_BadMessage.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public async Task QnaMaker_TraceActivity_BadMessage()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            // No text
            var adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(QnaMaker_TraceActivity_BadMessage)));
            var activity = new Activity
            {
                Type = ActivityTypes.Trace,
                Text = "My Text",
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount(),
            };
            var context = new TurnContext(adapter, activity);

            await Assert.ThrowsAsync<ArgumentException>(() => qna.GetAnswersAsync(context));
        }

        /// <summary>
        /// The QnaMaker_TraceActivity_NullActivity.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public async Task QnaMaker_TraceActivity_NullActivity()
        {
            // Get basic Qna
            var qna = QnaReturnsAnswer();

            // No text
            var adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(QnaMaker_TraceActivity_NullActivity)));
            var context = new MyTurnContext(adapter, null);

            await Assert.ThrowsAsync<ArgumentException>(() => qna.GetAnswersAsync(context));
        }

        /// <summary>
        /// The QnaMaker_ReturnsAnswer.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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
            Assert.NotNull(results);
            Assert.Single(results);
            Assert.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack", results[0].Answer);
        }

        /// <summary>
        /// The QnaMaker_ReturnsAnswerRaw.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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
            Assert.NotNull(results.Answers);
            Assert.True(results.ActiveLearningEnabled);
            Assert.Single(results.Answers);
            Assert.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack", results.Answers[0].Answer);
        }

        /// <summary>
        /// The QnaMaker_LowScoreVariation.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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
            Assert.NotNull(results);
            Assert.Equal(4, results.Length);

            var filteredResults = qna.GetLowScoreVariation(results);
            Assert.NotNull(filteredResults);
            Assert.Equal(3, filteredResults.Length);

            mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_TopNAnswer_DisableActiveLearning.json"));

            results = await qna.GetAnswersAsync(GetContext("Q11"));
            Assert.NotNull(results);
            Assert.Equal(4, results.Length);

            filteredResults = qna.GetLowScoreVariation(results);
            Assert.NotNull(filteredResults);
            Assert.Equal(3, filteredResults.Length);
        }

        /// <summary>
        /// The QnaMaker_CallTrain.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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

        /// <summary>
        /// The QnaMaker_ReturnsAnswer_Configuration.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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
            Assert.NotNull(results);
            Assert.Single(results);
            Assert.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack", results[0].Answer);
        }

        /// <summary>
        /// The QnaMaker_ReturnsAnswerWithFiltering.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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
            Assert.NotNull(results);
            Assert.Single(results);
            Assert.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack", results[0].Answer);
            Assert.Equal("topic", results[0].Metadata[0].Name);
            Assert.Equal("value", results[0].Metadata[0].Value);

            // verify we are actually passing on the options
            var obj = JObject.Parse(interceptHttp.Content);
            Assert.Equal(1, obj["top"].Value<int>());
            Assert.Equal("topic", obj["strictFilters"][0]["name"].Value<string>());
            Assert.Equal("value", obj["strictFilters"][0]["value"].Value<string>());
        }

        /// <summary>
        /// The QnaMaker_SetScoreThresholdWhenThresholdIsZero.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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

            Assert.NotNull(results);
            Assert.Single(results);
        }

        /// <summary>
        /// The QnaMaker_TestThreshold.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        /// <summary>
        /// The QnaMaker_Test_ScoreThresholdTooLarge_OutOfRange.
        /// </summary>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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

            Assert.Throws<ArgumentOutOfRangeException>(() => new QnAMaker(endpoint, tooLargeThreshold));
        }

        /// <summary>
        /// The QnaMaker_Test_ScoreThresholdTooSmall_OutOfRange.
        /// </summary>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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

            Assert.Throws<ArgumentOutOfRangeException>(() => new QnAMaker(endpoint, tooSmallThreshold));
        }

        /// <summary>
        /// The QnaMaker_ReturnsAnswerWithContext.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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
            Assert.NotNull(results);
            Assert.Single(results);
            Assert.Equal(55, results[0].Id);
            Assert.Equal(1, results[0].Score);
        }

        /// <summary>
        /// The QnaMaker_ReturnsAnswerWithoutContext.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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
            Assert.NotNull(results);
            Assert.Equal(2, results.Length);
            Assert.NotEqual(1, results[0].Score);
        }

        /// <summary>
        /// The QnaMaker_ReturnsHighScoreWhenIdPassed.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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
            Assert.NotNull(results);
            Assert.Single(results);
            Assert.Equal(55, results[0].Id);
            Assert.Equal(1, results[0].Score);
        }

        /// <summary>
        /// The QnaMaker_Test_Top_OutOfRange.
        /// </summary>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public void QnaMaker_Test_Top_OutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new QnAMaker(
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
                }));
        }

        /// <summary>
        /// The QnaMaker_Test_Endpoint_EmptyKbId.
        /// </summary>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public void QnaMaker_Test_Endpoint_EmptyKbId()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new QnAMaker(
                    new QnAMakerEndpoint
                    {
                        KnowledgeBaseId = string.Empty,
                        EndpointKey = _endpointKey,
                        Host = _hostname,
                    });
            });
        }

        /// <summary>
        /// The QnaMaker_Test_Endpoint_EmptyEndpointKey.
        /// </summary>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public void QnaMaker_Test_Endpoint_EmptyEndpointKey()
        {
            Assert.Throws<ArgumentException>(() => new QnAMaker(
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = string.Empty,
                    Host = _hostname,
                }));
        }

        /// <summary>
        /// The QnaMaker_Test_Endpoint_EmptyHost.
        /// </summary>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public void QnaMaker_Test_Endpoint_EmptyHost()
        {
            Assert.Throws<ArgumentException>(() => new QnAMaker(
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = string.Empty,
                }));
        }

        /// <summary>
        /// The QnaMaker_UserAgent.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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

            Assert.NotNull(results);
            Assert.Single(results);
            Assert.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack", results[0].Answer);

            // Verify that we added the bot.builder package details.
            var majorVersion = typeof(ConnectorClient).GetTypeInfo().Assembly.GetName().Version.Major;
            Assert.Contains($"Microsoft.Bot.Builder.AI.QnA/{majorVersion}", interceptHttp.UserAgent);
        }

        /// <summary>
        /// The QnaMaker_V2LegacyEndpoint_Should_Throw.
        /// </summary>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public void QnaMaker_V2LegacyEndpoint_Should_Throw()
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

            Assert.Throws<NotSupportedException>(() => GetQnAMaker(mockHttp, v2LegacyEndpoint));
        }

        /// <summary>
        /// The QnaMaker_V3LegacyEndpoint_ShouldThrow.
        /// </summary>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public void QnaMaker_V3LegacyEndpoint_ShouldThrow()
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

            Assert.Throws<NotSupportedException>(() => GetQnAMaker(mockHttp, v3LegacyEndpoint));
        }

        /// <summary>
        /// The QnaMaker_ReturnsAnswerWithMetadataBoost.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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
                Top = 1,
            };

            var results = await qna.GetAnswersAsync(GetContext("who loves me?"), options);

            Assert.NotNull(results);
            Assert.Single(results);
            Assert.StartsWith("Kiki", results[0].Answer);
        }

        /// <summary>
        /// The QnaMaker_TestThresholdInQueryOption.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public async Task QnaMaker_TestThresholdInQueryOption()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer_GivenScoreThresholdQueryOption.json"));

            var interceptHttp = new InterceptRequestHandler(mockHttp);

            var qna = GetQnAMaker(
                interceptHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                });

            var queryOptionsWithScoreThreshold = new QnAMakerOptions
            {
                ScoreThreshold = 0.5F,
                Top = 2,
            };

            var result = await qna.GetAnswersAsync(
                    GetContext("What happens when you hug a porcupine?"),
                    queryOptionsWithScoreThreshold);

            Assert.NotNull(result);

            var obj = JObject.Parse(interceptHttp.Content);
            Assert.Equal(2, obj["top"].Value<int>());
            Assert.Equal(0.5F, obj["scoreThreshold"].Value<float>() / 100.0f);
        }

        /// <summary>
        /// The QnaMaker_Test_UnsuccessfulResponse.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public async Task QnaMaker_Test_UnsuccessfulResponse()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond(HttpStatusCode.BadGateway);

            var qna = GetQnAMaker(
                mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowledgeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname,
                });

            await Assert.ThrowsAsync<HttpRequestException>(() => qna.GetAnswersAsync(GetContext("how do I clean the stove?")));
        }

        /// <summary>
        /// The QnaMaker_IsTest_True.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public async Task QnaMaker_IsTest_True()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_IsTest_True.json"));

            var qnaMakerOptions = new QnAMakerOptions
            {
                Top = 1,
                IsTest = true
            };
            var client = new HttpClient(mockHttp);

            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = _knowledgeBaseId,
                EndpointKey = _endpointKey,
                Host = _hostname,
            };

            var qna = new QnAMaker(endpoint, qnaMakerOptions, client, null, true);
            var results = await qna.GetAnswersAsync(GetContext("Will answer be any different now?"));

            // Assert - Validate we didn't break QnA functionality.
            Assert.NotNull(results);
            Assert.Single(results);
            Assert.StartsWith("No, isTest won't change your answer.", results[0].Answer);
        }

        /// <summary>
        /// The QnaMaker_RankerType_QuestionOnly.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
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

            var qnaMakerOptions = new QnAMakerOptions
            {
                Top = 1,
                RankerType = "QuestionOnly"
            };

            var results = await qna.GetAnswersAsync(GetContext("Q11"), qnaMakerOptions);
            Assert.NotNull(results);
            Assert.Equal(2, results.Length);
        }

        /// <summary>
        /// The QnaMaker_Test_Options_Hydration.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public async Task QnaMaker_Test_Options_Hydration()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var interceptHttp = new InterceptRequestHandler(mockHttp);

            var noFiltersOptions = new QnAMakerOptions
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

            var oneFilteredOption = new QnAMakerOptions
            {
                Top = 30,
                StrictFilters = new Metadata[]
                {
                    new Metadata
                    {
                        Name = "movie",
                        Value = "disney",
                    },
                },
            };

            var twoStrictFiltersOptions = new QnAMakerOptions
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
            var allChangedRequestOptions = new QnAMakerOptions
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
            await qna.GetAnswersAsync(context, noFiltersOptions);
            var requestContent1 = JsonConvert.DeserializeObject<CapturedRequest>(interceptHttp.Content);

            await qna.GetAnswersAsync(context, twoStrictFiltersOptions);
            var requestContent2 = JsonConvert.DeserializeObject<CapturedRequest>(interceptHttp.Content);

            await qna.GetAnswersAsync(context, oneFilteredOption);
            var requestContent3 = JsonConvert.DeserializeObject<CapturedRequest>(interceptHttp.Content);

            await qna.GetAnswersAsync(context);
            var requestContent4 = JsonConvert.DeserializeObject<CapturedRequest>(interceptHttp.Content);

            await qna.GetAnswersAsync(context, allChangedRequestOptions);
            var requestContent5 = JsonConvert.DeserializeObject<CapturedRequest>(interceptHttp.Content);

            await qna.GetAnswersAsync(context);
            var requestContent6 = JsonConvert.DeserializeObject<CapturedRequest>(interceptHttp.Content);

            Assert.Empty(requestContent1.StrictFilters);
            Assert.Equal(2, requestContent2.StrictFilters.Length);
            Assert.Single(requestContent3.StrictFilters);
            Assert.Empty(requestContent4.StrictFilters);

            Assert.Equal(2000, requestContent5.Top);
            Assert.Equal(42.0f, Math.Round(requestContent5.ScoreThreshold, 1));
            Assert.Single(requestContent5.StrictFilters);

            Assert.Equal(30, requestContent6.Top);
            Assert.Equal(30.0f, Math.Round(requestContent6.ScoreThreshold, 1));
            Assert.Empty(requestContent6.StrictFilters);
        }

        /// <summary>
        /// The QnaMaker_StrictFilters_Compound_OperationType.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        public async Task QnaMaker_StrictFilters_Compound_OperationType()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));

            var interceptHttp = new InterceptRequestHandler(mockHttp);
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
                    new Metadata()
                    {
                        Name = "production",
                        Value = "Walden",
                    },
                },
                StrictFiltersJoinOperator = JoinOperator.OR
            };
            var qna = GetQnAMaker(
                            interceptHttp,
                            new QnAMakerEndpoint
                            {
                                KnowledgeBaseId = _knowledgeBaseId,
                                EndpointKey = _endpointKey,

                                Host = _hostname,
                            }, oneFilteredOption);

            var context = GetContext("up");
            var noFilterResults1 = await qna.GetAnswersAsync(context, oneFilteredOption);
            var requestContent1 = JsonConvert.DeserializeObject<CapturedRequest>(interceptHttp.Content);
            Assert.Equal(2, oneFilteredOption.StrictFilters.Length);
            Assert.Equal(JoinOperator.OR, oneFilteredOption.StrictFiltersJoinOperator);
        }

        /// <summary>
        /// The Telemetry_NullTelemetryClient.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        [Trait("TestCategory", "Telemetry")]
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
            // This will default to the NullTelemetryClient which no-ops all calls.
            var qna = new QnAMaker(endpoint, options, client, null, true);
            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));

            // Assert - Validate we didn't break QnA functionality.
            Assert.NotNull(results);
            Assert.Single(results);
            Assert.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack", results[0].Answer);
            Assert.StartsWith("Editorial", results[0].Source);
        }

        /// <summary>
        /// The Telemetry_ReturnsAnswer.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        [Trait("TestCategory", "Telemetry")]
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
            Assert.Equal(1, telemetryClient.Invocations.Count);
            Assert.Equal(3, telemetryClient.Invocations[0].Arguments.Count);
            Assert.Equal(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("knowledgeBaseId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("matchedQuestion"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("question"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("questionId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("answer"));
            Assert.Equal("BaseCamp: You can use a damp rag to clean around the Power Pack", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["answer"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("articleFound"));
            Assert.Single((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]);
            Assert.True(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("score"));

            // Assert - Validate we didn't break QnA functionality.
            Assert.NotNull(results);
            Assert.Single(results);
            Assert.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack", results[0].Answer);
            Assert.StartsWith("Editorial", results[0].Source);
        }

        /// <summary>
        /// The Telemetry_ReturnsAnswer_WhenNoAnswerFoundInKB.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        [Trait("TestCategory", "Telemetry")]
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
            Assert.Equal(1, telemetryClient.Invocations.Count);
            Assert.Equal(3, telemetryClient.Invocations[0].Arguments.Count);
            Assert.Equal(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("knowledgeBaseId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("matchedQuestion"));
            Assert.Equal("No good match found in KB.", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["answer"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("question"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("questionId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("answer"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("articleFound"));

            // Assert - Validate we didn't break QnA functionality.
            Assert.NotNull(results);
        }

        /// <summary>
        /// The Telemetry_PII.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        [Trait("TestCategory", "Telemetry")]
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
            Assert.Single(telemetryClient.Invocations);
            Assert.Equal(3, telemetryClient.Invocations[0].Arguments.Count);
            Assert.Equal(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("knowledgeBaseId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("matchedQuestion"));
            Assert.False(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("question"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("questionId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("answer"));
            Assert.Equal("BaseCamp: You can use a damp rag to clean around the Power Pack", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["answer"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("articleFound"));
            Assert.Single((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]);
            Assert.True(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("score"));

            // Assert - Validate we didn't break QnA functionality.
            Assert.NotNull(results);
            Assert.Single(results);
            Assert.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack", results[0].Answer);
            Assert.StartsWith("Editorial", results[0].Source);
        }

        /// <summary>
        /// The Telemetry_Override.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        [Trait("TestCategory", "Telemetry")]
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
            Assert.Equal(2, telemetryClient.Invocations.Count);
            Assert.Equal(3, telemetryClient.Invocations[0].Arguments.Count);
            Assert.Equal(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).Count == 2);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("MyImportantProperty"));
            Assert.Equal("myImportantValue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["MyImportantProperty"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("Id"));
            Assert.Equal("MyID", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["Id"]);

            Assert.Equal("MySecondEvent", telemetryClient.Invocations[1].Arguments[0]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[1].Arguments[1]).ContainsKey("MyImportantProperty2"));
            Assert.Equal("myImportantValue2", ((Dictionary<string, string>)telemetryClient.Invocations[1].Arguments[1])["MyImportantProperty2"]);

            // Validate we didn't break QnA functionality.
            Assert.NotNull(results);
            Assert.Single(results);
            Assert.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack", results[0].Answer);
            Assert.StartsWith("Editorial", results[0].Source);
        }

        /// <summary>
        /// The Telemetry_AdditionalPropsMetrics.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        [Trait("TestCategory", "Telemetry")]
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
            Assert.Equal(1, telemetryClient.Invocations.Count);
            Assert.Equal(3, telemetryClient.Invocations[0].Arguments.Count);
            Assert.Equal(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey(QnATelemetryConstants.KnowledgeBaseIdProperty));
            Assert.False(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey(QnATelemetryConstants.QuestionProperty));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey(QnATelemetryConstants.MatchedQuestionProperty));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey(QnATelemetryConstants.QuestionIdProperty));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey(QnATelemetryConstants.AnswerProperty));
            Assert.Equal("BaseCamp: You can use a damp rag to clean around the Power Pack", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["answer"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("MyImportantProperty"));
            Assert.Equal("myImportantValue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["MyImportantProperty"]);

            Assert.Equal(2, ((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).Count);
            Assert.True(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("score"));
            Assert.True(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("MyImportantMetric"));
            Assert.Equal(3.14159, ((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2])["MyImportantMetric"]);

            // Validate we didn't break QnA functionality.
            Assert.NotNull(results);
            Assert.Single(results);
            Assert.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack", results[0].Answer);
            Assert.StartsWith("Editorial", results[0].Source);
        }

        /// <summary>
        /// The Telemetry_AdditionalPropsOverride.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        [Trait("TestCategory", "Telemetry")]
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

            await qna.GetAnswersAsync(GetContext("how do I clean the stove?"), null, telemetryProperties, telemetryMetrics);

            // Assert - added properties were added.
            Assert.Equal(1, telemetryClient.Invocations.Count);
            Assert.Equal(3, telemetryClient.Invocations[0].Arguments.Count);
            Assert.Equal(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("knowledgeBaseId"));
            Assert.Equal("myImportantValue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["knowledgeBaseId"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("matchedQuestion"));
            Assert.Equal("myImportantValue2", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["originalQuestion"]);
            Assert.False(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("question"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("questionId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("answer"));
            Assert.Equal("BaseCamp: You can use a damp rag to clean around the Power Pack", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["answer"]);
            Assert.False(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("MyImportantProperty"));

            Assert.Single((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]);
            Assert.True(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("score"));
            Assert.Equal(3.14159, ((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2])["score"]);
        }

        /// <summary>
        /// The Telemetry_FillPropsOverride.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnaMaker")]
        [Trait("TestCategory", "Telemetry")]
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

            await qna.GetAnswersAsync(GetContext("how do I clean the stove?"), null, telemetryProperties, telemetryMetrics);

            // Assert - added properties were added.
            Assert.Equal(2, telemetryClient.Invocations.Count);
            Assert.Equal(3, telemetryClient.Invocations[0].Arguments.Count);
            Assert.Equal(telemetryClient.Invocations[0].Arguments[0], QnATelemetryConstants.QnaMsgEvent);
            Assert.Equal(6, ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).Count);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("knowledgeBaseId"));
            Assert.Equal("myImportantValue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["knowledgeBaseId"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("matchedQuestion"));
            Assert.Equal("myImportantValue2", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["matchedQuestion"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("questionId"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("answer"));
            Assert.Equal("BaseCamp: You can use a damp rag to clean around the Power Pack", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["answer"]);
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("articleFound"));
            Assert.True(((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1]).ContainsKey("MyImportantProperty"));
            Assert.Equal("myImportantValue", ((Dictionary<string, string>)telemetryClient.Invocations[0].Arguments[1])["MyImportantProperty"]);

            Assert.Single((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]);
            Assert.True(((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2]).ContainsKey("score"));
            Assert.Equal(3.14159, ((Dictionary<string, double>)telemetryClient.Invocations[0].Arguments[2])["score"]);
        }

        /// <summary>
        /// The GetContext.
        /// </summary>
        /// <param name="utterance">The utterance<see cref="string"/>.</param>
        /// <returns>The <see cref="TurnContext"/>.</returns>
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

        /// <summary>
        /// The CreateFlow.
        /// </summary>
        /// <param name="rootDialog">The rootDialog<see cref="Dialog"/>.</param>
        /// <param name="testName">The testName<see cref="string"/>.</param>
        /// <returns>The <see cref="TestFlow"/>.</returns>
        private TestFlow CreateFlow(Dialog rootDialog, string testName)
        {
            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(testName));
            adapter
                .UseStorage(storage)
                .UseBotState(userState, conversationState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(false)));

            var dm = new DialogManager(rootDialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Defines the <see cref="QnaMakerTestDialog" />.
        /// </summary>
        public class QnaMakerTestDialog : ComponentDialog, IDialogDependencies
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QnaMakerTestDialog"/> class.
            /// </summary>
            /// <param name="knowledgeBaseId">The knowledgeBaseId<see cref="string"/>.</param>
            /// <param name="endpointKey">The endpointKey<see cref="string"/>.</param>
            /// <param name="hostName">The hostName<see cref="string"/>.</param>
            /// <param name="httpClient">The httpClient<see cref="HttpClient"/>.</param>
            public QnaMakerTestDialog(string knowledgeBaseId, string endpointKey, string hostName, HttpClient httpClient)
                : base(nameof(QnaMakerTestDialog))
            {
                AddDialog(new QnAMakerDialog(knowledgeBaseId, endpointKey, hostName, httpClient: httpClient));
            }

            /// <summary>
            /// The BeginDialogAsync.
            /// </summary>
            /// <param name="outerDc">The outerDc<see cref="DialogContext"/>.</param>
            /// <param name="options">The options<see cref="object"/>.</param>
            /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
            /// <returns>The <see cref="Task{DialogTurnResult}"/>.</returns>
            public override Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default)
            {
                return ContinueDialogAsync(outerDc, cancellationToken);
            }

            /// <summary>
            /// The ContinueDialogAsync.
            /// </summary>
            /// <param name="dc">The dc<see cref="DialogContext"/>.</param>
            /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
            /// <returns>The <see cref="Task{DialogTurnResult}"/>.</returns>
            public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
            {
                if (dc.Context.Activity.Text == "moo")
                {
                    await dc.Context.SendActivityAsync("Yippee ki-yay!");
                    return EndOfTurn;
                }

                return await dc.BeginDialogAsync("qnaDialog");
            }

            /// <summary>
            /// The GetDependencies.
            /// </summary>
            /// <returns>The <see cref="IEnumerable{Dialog}"/>.</returns>
            public IEnumerable<Dialog> GetDependencies()
            {
                return Dialogs.GetDialogs();
            }

            /// <summary>
            /// The ResumeDialogAsync.
            /// </summary>
            /// <param name="dc">The dc<see cref="DialogContext"/>.</param>
            /// <param name="reason">The reason<see cref="DialogReason"/>.</param>
            /// <param name="result">The result<see cref="object"/>.</param>
            /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
            /// <returns>The <see cref="Task{DialogTurnResult}"/>.</returns>
            public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
            {
                if ((bool)result == false)
                {
                    await dc.Context.SendActivityAsync("I didn't understand that.");
                }

                return await base.ResumeDialogAsync(dc, reason, result, cancellationToken);
            }
        }

        /// <summary>
        /// The CreateQnAMakerActionDialog.
        /// </summary>
        /// <param name="mockHttp">The mockHttp<see cref="MockHttpMessageHandler"/>.</param>
        /// <returns>The <see cref="AdaptiveDialog"/>.</returns>
        private AdaptiveDialog CreateQnAMakerActionDialog(MockHttpMessageHandler mockHttp)
        {
            var client = new HttpClient(mockHttp);

            var noAnswerActivity = new ActivityTemplate("No match found, please ask another question.");
            const string host = "https://dummy-hostname.azurewebsites.net/qnamaker";
            const string knowledgeBaseId = "dummy-id";
            const string endpointKey = "dummy-key";
            const string activeLearningCardTitle = "QnAMaker Active Learning";

            var outerDialog = new AdaptiveDialog("outer")
            {
                AutoEndDialog = false,
                Triggers = new List<OnCondition>
                {
                    new OnBeginDialog
                    {
                        Actions = new List<Dialog>
                        {
                            new QnAMakerDialog
                            {
                                KnowledgeBaseId = knowledgeBaseId,
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
                Triggers = new List<OnCondition>
                {
                    new OnBeginDialog
                    {
                        Actions = new List<Dialog>
                        {
                            new BeginDialog(outerDialog.Id)
                        }
                    },
                    new OnDialogEvent
                    {
                        Event = "UnhandledUnknownIntent",
                        Actions = new List<Dialog>
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

        /// <summary>
        /// The GetV2LegacyRequestUrl.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetV2LegacyRequestUrl() => $"{_hostname}/v2.0/knowledgebases/{_knowledgeBaseId}/generateanswer";

        /// <summary>
        /// The GetV3LegacyRequestUrl.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetV3LegacyRequestUrl() => $"{_hostname}/v3.0/knowledgebases/{_knowledgeBaseId}/generateanswer";

        /// <summary>
        /// The GetRequestUrl.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetRequestUrl() => $"{_hostname}/knowledgebases/{_knowledgeBaseId}/generateanswer";

        /// <summary>
        /// The GetTrainRequestUrl.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetTrainRequestUrl() => $"{_hostname}/knowledgebases/{_knowledgeBaseId}/train";

        /// <summary>
        /// The GetResponse.
        /// </summary>
        /// <param name="fileName">The fileName<see cref="string"/>.</param>
        /// <returns>The <see cref="Stream"/>.</returns>
        private Stream GetResponse(string fileName)
        {
            var path = GetFilePath(fileName);
            return File.OpenRead(path);
        }

        /// <summary>
        /// The GetFilePath.
        /// </summary>
        /// <param name="fileName">The fileName<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetFilePath(string fileName)
        {
            return Path.Combine(Environment.CurrentDirectory, "TestData/QnaMaker", fileName);
        }

        /// <summary>
        /// Return a stock Mocked Qna thats loaded with QnaMaker_ReturnsAnswer.json
        /// Used for tests that just require any old qna instance.
        /// </summary>
        /// <returns>The <see cref="QnAMaker"/>.</returns>
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

        /// <summary>
        /// The GetQnAMaker.
        /// </summary>
        /// <param name="messageHandler">The messageHandler<see cref="HttpMessageHandler"/>.</param>
        /// <param name="endpoint">The endpoint<see cref="QnAMakerEndpoint"/>.</param>
        /// <param name="options">The options<see cref="QnAMakerOptions"/>.</param>
        /// <returns>The <see cref="QnAMaker"/>.</returns>
        private QnAMaker GetQnAMaker(HttpMessageHandler messageHandler, QnAMakerEndpoint endpoint, QnAMakerOptions options = null)
        {
            var client = new HttpClient(messageHandler);
            return new QnAMaker(endpoint, options, client);
        }

        /// <summary>
        /// Defines the <see cref="OverrideTelemetry" />.
        /// </summary>
        public class OverrideTelemetry : QnAMaker
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OverrideTelemetry"/> class.
            /// </summary>
            /// <param name="endpoint">The endpoint<see cref="QnAMakerEndpoint"/>.</param>
            /// <param name="options">The options<see cref="QnAMakerOptions"/>.</param>
            /// <param name="httpClient">The httpClient<see cref="HttpClient"/>.</param>
            /// <param name="telemetryClient">The telemetryClient<see cref="IBotTelemetryClient"/>.</param>
            /// <param name="logPersonalInformation">The logPersonalInformation<see cref="bool"/>.</param>
            public OverrideTelemetry(QnAMakerEndpoint endpoint, QnAMakerOptions options, HttpClient httpClient, IBotTelemetryClient telemetryClient, bool logPersonalInformation)
                : base(endpoint, options, httpClient, telemetryClient, logPersonalInformation)
            {
            }

            /// <summary>
            /// The OnQnaResultsAsync.
            /// </summary>
            /// <param name="queryResults">The queryResults<see cref="QueryResult[]"/>.</param>
            /// <param name="turnContext">The turnContext<see cref="ITurnContext"/>.</param>
            /// <param name="telemetryProperties">The telemetryProperties<see cref="Dictionary{string, string}"/>.</param>
            /// <param name="telemetryMetrics">The telemetryMetrics<see cref="Dictionary{string, double}"/>.</param>
            /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
            /// <returns>The <see cref="Task"/>.</returns>
            protected override Task OnQnaResultsAsync(
                                        QueryResult[] queryResults,
                                        ITurnContext turnContext,
                                        Dictionary<string, string> telemetryProperties = null,
                                        Dictionary<string, double> telemetryMetrics = null,
                                        CancellationToken cancellationToken = default)
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

        /// <summary>
        /// Defines the <see cref="OverrideFillTelemetry" />.
        /// </summary>
        public class OverrideFillTelemetry : QnAMaker
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OverrideFillTelemetry"/> class.
            /// </summary>
            /// <param name="endpoint">The endpoint<see cref="QnAMakerEndpoint"/>.</param>
            /// <param name="options">The options<see cref="QnAMakerOptions"/>.</param>
            /// <param name="httpClient">The httpClient<see cref="HttpClient"/>.</param>
            /// <param name="telemetryClient">The telemetryClient<see cref="IBotTelemetryClient"/>.</param>
            /// <param name="logPersonalInformation">The logPersonalInformation<see cref="bool"/>.</param>
            public OverrideFillTelemetry(QnAMakerEndpoint endpoint, QnAMakerOptions options, HttpClient httpClient, IBotTelemetryClient telemetryClient, bool logPersonalInformation)
                : base(endpoint, options, httpClient, telemetryClient, logPersonalInformation)
            {
            }

            /// <summary>
            /// The OnQnaResultsAsync.
            /// </summary>
            /// <param name="queryResults">The queryResults<see cref="QueryResult[]"/>.</param>
            /// <param name="turnContext">The turnContext<see cref="ITurnContext"/>.</param>
            /// <param name="telemetryProperties">The telemetryProperties<see cref="Dictionary{string, string}"/>.</param>
            /// <param name="telemetryMetrics">The telemetryMetrics<see cref="Dictionary{string, double}"/>.</param>
            /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
            /// <returns>The <see cref="Task"/>.</returns>
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

        /// <summary>
        /// Defines the <see cref="CapturedRequest" />.
        /// </summary>
        private class CapturedRequest
        {
            /// <summary>
            /// Gets or sets the Questions.
            /// </summary>
            public string[] Questions { get; set; }

            /// <summary>
            /// Gets or sets the Top.
            /// </summary>
            public int Top { get; set; }

            /// <summary>
            /// Gets or sets the StrictFilters.
            /// </summary>
            public Metadata[] StrictFilters { get; set; }

            /// <summary>
            /// Gets or sets the MetadataBoost.
            /// </summary>
            public Metadata[] MetadataBoost { get; set; }

            /// <summary>
            /// Gets or sets the ScoreThreshold.
            /// </summary>
            public float ScoreThreshold { get; set; }
        }
    }
}
