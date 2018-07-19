// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.Ai.QnA.Tests
{
    [TestClass]
    public class QnAMakerTests
    {
        public readonly string knowlegeBaseId = TestUtilities.GetKey("QNAKNOWLEDGEBASEID") ?? "dummy-id";
        public readonly string endpointKey = TestUtilities.GetKey("QNAENDPOINTKEY") ?? "dummy-key";
        public readonly string hostname = TestUtilities.GetKey("QNAHOSTNAME") ?? "https://dummy-hostname.azurewebsites.net/qnamaker";


        private string GetRequestUrl()
        {
            return $"{hostname}/knowledgebases/{knowlegeBaseId}/generateanswer";
        }

        private Stream GetResponse(string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "TestData", fileName);
            return File.OpenRead(path);
        }

        private string GetFilePath(string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "TestData", fileName);
            return path;
        }

        private QnAMaker GetQnAMaker(HttpMessageHandler messageHandler, QnAMakerEndpoint endpoint, QnAMakerOptions options = null)
        {
            HttpClient client = null;
            if (!EnvironmentVariablesDefined())
            {
                client = new HttpClient(messageHandler);
            }
            return new QnAMaker(endpoint, options, client);
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
                    KnowledgeBaseId = knowlegeBaseId,
                    EndpointKey = endpointKey,
                    Host = hostname
                },
                new QnAMakerOptions
                {
                    Top = 1
                });

            var results = await qna.GetAnswers("how do I clean the stove?");
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
                    KnowledgeBaseId = knowlegeBaseId,
                    EndpointKey = endpointKey,
                    Host = hostname
                },
                new QnAMakerOptions
                {
                    Top = 1,
                    ScoreThreshold = 0.99F
                });

            var results = await qna.GetAnswers("how do I clean the stove?");
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
                    KnowledgeBaseId = knowlegeBaseId,
                    EndpointKey = endpointKey,
                    Host = hostname
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
                    KnowledgeBaseId = knowlegeBaseId,
                    EndpointKey = endpointKey,
                    Host = hostname
                },
                new QnAMakerOptions
                {
                    Top = -1,
                    ScoreThreshold = 0.5F
                });
        }

        private bool EnvironmentVariablesDefined()
        {
            return TestUtilities.GetKey("QNAKNOWLEDGEBASEID") != null
                && TestUtilities.GetKey("QNAENDPOINTKEY") != null
                && TestUtilities.GetKey("QNAHOSTNAME") != null;
        }
    }
}
