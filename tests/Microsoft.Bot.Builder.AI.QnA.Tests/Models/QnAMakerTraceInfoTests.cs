﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.AI.QnA.Tests.Models
{
    public class QnAMakerTraceInfoTests
    {
        [Fact]
        [Trait("TestCategory", "AI")]
        [Trait("TestCategory", "QnAMaker")]
        public void QnAMakerTraceInfo_Serialization()
        {
            var qnaMakerTraceInfo = new QnAMakerTraceInfo
            {
                KnowledgeBaseId = Guid.NewGuid().ToString(),
                ScoreThreshold = 0.5F,
                Top = 1,
            };

            var result = new QueryResult()
            {
                Answer = "My name is Mike",
                Score = 0.9F,
            };

            result.Questions.Add("What's your name?");

            qnaMakerTraceInfo.QueryResults.Add(result);

            var serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var serialized = JsonConvert.SerializeObject(qnaMakerTraceInfo, serializerSettings);
            var deserialized = JsonConvert.DeserializeObject<QnAMakerTraceInfo>(serialized, serializerSettings);

            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.QueryResults);
            Assert.NotNull(deserialized.KnowledgeBaseId);
            Assert.Equal(0.5, deserialized.ScoreThreshold);
            Assert.Equal(1, deserialized.Top);
            Assert.Equal(qnaMakerTraceInfo.QueryResults[0].Questions[0], deserialized.QueryResults[0].Questions[0]);
            Assert.Equal(qnaMakerTraceInfo.QueryResults[0].Answer, deserialized.QueryResults[0].Answer);
            Assert.Equal(qnaMakerTraceInfo.KnowledgeBaseId, deserialized.KnowledgeBaseId);
            Assert.Equal(qnaMakerTraceInfo.ScoreThreshold, deserialized.ScoreThreshold);
            Assert.Equal(qnaMakerTraceInfo.Top, deserialized.Top);
        }
    }
}
