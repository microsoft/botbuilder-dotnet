// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net.Http;
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

            await new TestFlow(adapter, async (context) =>
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
}
