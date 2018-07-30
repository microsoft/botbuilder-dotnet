// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.Ai.QnA.Tests
{
    [TestClass]
    public class QnAMakerTests
    {
        private const string _knowlegeBaseId = "dummy-id";
        private const string _endpointKey = "dummy-key";
        private const string _hostname = "https://dummy-hostname.azurewebsites.net/qnamaker";


        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TraceActivity()
        {
            // Mock Qna
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));
            var qna = GetQnAMaker(mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions
                {
                    Top = 1
                });

            // Invoke flow which uses mock
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
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
                    RelatesTo = context.Activity.RelatesTo
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
            QnAMakerTraceInfo traceInfo = ((JObject)((ITraceActivity)pagedResult.Items[1]).Value).ToObject<QnAMakerTraceInfo>();
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
            var adapter = new TestAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "",
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount()
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
            var adapter = new TestAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = null,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount()
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
            var adapter = new TestAdapter();
            var activity = new Activity
            {
                Type = ActivityTypes.Trace,
                Text = "My Text",
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount()
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
            var adapter = new TestAdapter();
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

            var qna = GetQnAMaker(mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions
                {
                    Top = 1
                });

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            StringAssert.StartsWith(results[0].Answer, "BaseCamp: You can use a damp rag to clean around the Power Pack");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TestThreshold()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                .Respond("application/json", GetResponse("QnaMaker_TestThreshold.json"));

            var qna = GetQnAMaker(mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions
                {
                    Top = 1,
                    ScoreThreshold = 0.99F
                });

            var results = await qna.GetAnswersAsync(GetContext("how do I clean the stove?"));
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 0, "should get zero result because threshold");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void QnaMaker_Test_ScoreThreshold_OutOfRange()
        {
            var qna = new QnAMaker(
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions
                {
                    Top = 1,
                    ScoreThreshold = 1.1F
                });
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
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions
                {
                    Top = -1,
                    ScoreThreshold = 0.5F
                });
        }

        private string GetRequestUrl()
        {
            return $"{_hostname}/knowledgebases/{_knowlegeBaseId}/generateanswer";
        }

        private Stream GetResponse(string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "TestData", fileName);
            return File.OpenRead(path);
        }

        /// <summary>
        /// Return a stock Mocked Qna thats loaded with QnaMaker_ReturnsAnswer.json
        /// 
        /// Used for tests that just require any old qna instance
        /// </summary>
        /// <returns></returns>
        private QnAMaker QnaReturnsAnswer()
        {
            // Mock Qna
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, GetRequestUrl())
                    .Respond("application/json", GetResponse("QnaMaker_ReturnsAnswer.json"));
            var qna = GetQnAMaker(mockHttp,
                new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _knowlegeBaseId,
                    EndpointKey = _endpointKey,
                    Host = _hostname
                },
                new QnAMakerOptions
                {
                    Top = 1
                });
            return qna;
        }


        private QnAMaker GetQnAMaker(HttpMessageHandler messageHandler, QnAMakerEndpoint endpoint, QnAMakerOptions options = null)
        {
            var client = new HttpClient(messageHandler);
            return new QnAMaker(endpoint, options, client);
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
                From = new ChannelAccount()
            };
            return new TurnContext(b, a);

        }
    }

    class MyTurnContext : ITurnContext
    {

        public MyTurnContext(BotAdapter adapter, Activity activity)
        {
            Activity = activity;
            Adapter = adapter;
        }
        public BotAdapter Adapter { get; }

        public TurnContextServiceCollection Services => throw new NotImplementedException();

        public Activity Activity { get; }

        public bool Responded => throw new NotImplementedException();

        public Task DeleteActivityAsync(string activityId, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task DeleteActivityAsync(ConversationReference conversationReference, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
        {
            throw new NotImplementedException();
        }

        public ITurnContext OnSendActivities(SendActivitiesHandler handler)
        {
            throw new NotImplementedException();
        }

        public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
        {
            throw new NotImplementedException();
        }

        public Task<ResourceResponse[]> SendActivitiesAsync(IActivity[] activities, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<ResourceResponse> SendActivityAsync(string textReplyToSend, string speak = null, string inputHint = "acceptingInput", CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<ResourceResponse> SendActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<ResourceResponse> UpdateActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }


}
