// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Ai.QnA.Tests
{
    [TestClass]
    public class QnAMakerTests
    {
        public readonly string knowlegeBaseId = TestUtilities.GetKey("QNAKNOWLEDGEBASEID");
        public readonly string subscriptionKey = TestUtilities.GetKey("QNASUBSCRIPTIONKEY");

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswer()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing QnaMaker Environment variables - Skipping test");
                return;
            }

            var qna = new QnAMaker(new QnAMakerOptions()
            {
                KnowledgeBaseId = knowlegeBaseId,
                SubscriptionKey = subscriptionKey,
                Top = 1
            }, new HttpClient());

            var results = await qna.GetAnswers("how do I clean the stove?");
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 1, "should get one result");
            Assert.IsTrue(results[0].Answer.StartsWith("BaseCamp: You can use a damp rag to clean around the Power Pack"));
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TestThreshold()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing QnaMaker Environment variables - Skipping test");
                return;
            }

            var qna = new QnAMaker(new QnAMakerOptions()
            {
                KnowledgeBaseId = knowlegeBaseId,
                SubscriptionKey = subscriptionKey,
                Top = 1,
                ScoreThreshold = 0.99F
            }, new HttpClient());

            var results = await qna.GetAnswers("how do I clean the stove?");
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Length, 0, "should get zero result because threshold");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task QnaMaker_Test_ScoreThreshold_OutOfRange()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing QnaMaker Environment variables - Skipping test");
                return;
            }

            var qna = new QnAMaker(new QnAMakerOptions()
            {
                KnowledgeBaseId = knowlegeBaseId,
                SubscriptionKey = subscriptionKey,
                Top = 1,
                ScoreThreshold = 1.1F
            }, new HttpClient());

            var results = await qna.GetAnswers("how do I clean the stove?");
        }

        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task QnaMaker_Test_Top_OutOfRange()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing QnaMaker Environment variables - Skipping test");
                return;
            }

            var qna = new QnAMaker(new QnAMakerOptions()
            {
                KnowledgeBaseId = knowlegeBaseId,
                SubscriptionKey = subscriptionKey,
                Top = -1,
                ScoreThreshold = 0.5F
            }, new HttpClient());

            var results = await qna.GetAnswers("how do I clean the stove?");
        }

        private bool EnvironmentVariablesDefined()
        {
            return knowlegeBaseId != null && subscriptionKey != null;
        }
    }
}
