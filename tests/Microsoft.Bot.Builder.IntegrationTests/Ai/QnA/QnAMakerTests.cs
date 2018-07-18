// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Ai.QnA;
using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.IntegrationTests.Ai.QnA
{
#if !RUNINTEGRATIONTESTS
    [Ignore("These integration tests run only when RUNINTEGRATIONTESTS is defined")]
#endif
    [TestClass]
    public class QnAMakerTests
    {
        public readonly string knowlegeBaseId = TestUtilities.GetKey("QNAKNOWLEDGEBASEID");
        public readonly string endpointKey = TestUtilities.GetKey("QNAENDPOINTKEY");
        public readonly string hostname = TestUtilities.GetKey("QNAHOSTNAME");

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [Ignore]
        public async Task QnaMaker_ReturnsAnswer()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing QnaMaker Environment variables - Skipping test");
                return;
            }

            var qna = new QnAMaker(
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

            var results = await qna.GetAnswersAsync("how do I clean the stove?");
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            Assert.IsTrue(results[0].Answer.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack"));
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [Ignore]
        public async Task QnaMaker_TestThreshold()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing QnaMaker Environment variables - Skipping test");
                return;
            }

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
                    ScoreThreshold = 0.99F
                });

            var results = await qna.GetAnswersAsync("how do I clean the stove?");
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 0, "should get zero result because threshold");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Ignore]
        public void QnaMaker_Test_ScoreThreshold_OutOfRange()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing QnaMaker Environment variables - Skipping test");
                return;
            }

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
        [Ignore]
        public void QnaMaker_Test_Top_OutOfRange()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing QnaMaker Environment variables - Skipping test");
                return;
            }

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
            return knowlegeBaseId != null && endpointKey != null && hostname != null;
        }
    }
}
