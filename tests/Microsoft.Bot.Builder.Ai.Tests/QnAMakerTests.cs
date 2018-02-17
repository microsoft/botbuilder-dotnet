// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai.Tests
{
    [TestClass]
    public class QnAMakerTests
    {
        public string knowlegeBaseId = TestUtilities.GetKey("QNAKNOWLEDGEBASEID");
        public string subscriptionKey = TestUtilities.GetKey("QNASUBSCRIPTIONKEY");

        //[TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_ReturnsAnswer()
        {
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

        //[TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public async Task QnaMaker_TestThreshold()
        {

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
    }
}
