// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Ai.Tests
{
    [TestClass]
    public class SmartTalkTests
    {
        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("SmartTalk")]
        public async Task SmartTalk_ReturnsAnswer()
        {
            var smartTalk = new SmartTalk(new SmartTalkMiddlewareOptions(), new HttpClient());

            var results = await smartTalk.GetAnswers("test query aswedff");
            Assert.IsNotNull(results);
            Assert.AreEqual(results.ScenarioList[0].Responses[0], "test response", "Smart talk didn't return correct response.");
        }
    }
}