// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.QnA.Tests
{
    [TestClass]
    public class QnAMakerTraceInfoTests
    {
        [TestMethod]
        [TestCategory("AI")]
        [TestCategory("QnAMaker")]
        public void QnAMakerTraceInfo_Serialization()
        {
            var qnaMakerTraceInfo = new QnAMakerTraceInfo
            {
                QueryResults = new QueryResult[] 
                {
                    new QueryResult
                    {
                        Questions = new string[] { "What's your name?" },
                        Answer = "My name is Mike",
                        Score = 0.9F,
                    }
                },
                KnowledgeBaseId = Guid.NewGuid().ToString(),
                ScoreThreshold = 0.5F,
                Top = 1
            };

            var serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var serialized = JsonConvert.SerializeObject(qnaMakerTraceInfo, serializerSettings);
            var deserialized = JsonConvert.DeserializeObject<QnAMakerTraceInfo>(serialized, serializerSettings);

            Assert.IsNotNull(deserialized);
            Assert.IsNotNull(deserialized.QueryResults);
            Assert.IsNotNull(deserialized.KnowledgeBaseId);
            Assert.IsNotNull(deserialized.ScoreThreshold);
            Assert.IsNotNull(deserialized.Top);
            Assert.AreEqual(qnaMakerTraceInfo.QueryResults[0].Questions[0], deserialized.QueryResults[0].Questions[0]);
            Assert.AreEqual(qnaMakerTraceInfo.QueryResults[0].Answer, deserialized.QueryResults[0].Answer);
            Assert.AreEqual(qnaMakerTraceInfo.KnowledgeBaseId, deserialized.KnowledgeBaseId);
            Assert.AreEqual(qnaMakerTraceInfo.ScoreThreshold, deserialized.ScoreThreshold);
            Assert.AreEqual(qnaMakerTraceInfo.Top, deserialized.Top);
        }
    }
}
