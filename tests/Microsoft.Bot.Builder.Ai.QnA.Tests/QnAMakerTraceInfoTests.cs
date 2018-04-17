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
                QnAMakerOptions = new QnAMakerOptions
                {
                    ScoreThreshold = 0.5F,
                    Top = 1
                }
            };

            var serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var serialized = JsonConvert.SerializeObject(qnaMakerTraceInfo, serializerSettings);
            var deserialized = JsonConvert.DeserializeObject<QnAMakerTraceInfo>(serialized, serializerSettings);

            Assert.IsNotNull(deserialized);
            Assert.IsNotNull(deserialized.QueryResults);
            Assert.IsNotNull(deserialized.QnAMakerOptions);
            Assert.AreEqual(qnaMakerTraceInfo.QueryResults[0].Questions[0], deserialized.QueryResults[0].Questions[0]);
            Assert.AreEqual(qnaMakerTraceInfo.QueryResults[0].Answer, deserialized.QueryResults[0].Answer);
            Assert.AreEqual(qnaMakerTraceInfo.QnAMakerOptions.ScoreThreshold, deserialized.QnAMakerOptions.ScoreThreshold);
        }
    }
}
