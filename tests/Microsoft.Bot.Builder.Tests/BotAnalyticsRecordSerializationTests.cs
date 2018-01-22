using Microsoft.Bot.Builder.Analytics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Analytics")]
    [TestCategory("JSON Serialization")]
    public class BotAnalyticsRecordSerializationTests
    {
        [TestMethod]
        public void SerializeToJson()
        {
            DateTime startTime = DateTime.Parse("2017-08-25T03:49:00-07:00");

            BotAnalyticsRecord r = new BotAnalyticsRecord
            {
                BotId = "12345",
                Context = "http://testContext",
                id = "11111",
                ReceivedAtDateTime = startTime,
                Type = "http://testType"
            };
            

            string s = JsonConvert.SerializeObject((dynamic)r, Formatting.Indented);
            var serializedRecord = JToken.Parse(s);

            string targetJson = @"
            {
                '@context' : 'http://testContext',
                '@type' : 'http://testType', 
                '@id' : '11111',
                'receivedAtDateTime' : '2017-08-25T03:49:00-07:00',
                'botId' : '12345'
            }";
            var desiredResult = JToken.Parse(targetJson);

            Assert.IsTrue(JToken.DeepEquals(desiredResult, serializedRecord), "Two JSON Objects are not equal");
        }

        [TestMethod]
        public void FacetSerializer()
        {
            DateTime startTime = DateTime.Parse("2017-08-25T03:49:00-07:00");

            BotAnalyticsRecord r = new BotAnalyticsRecord
            {
                BotId = "12345",
                Context = "http://testContext",
                id = "11111",
                ReceivedAtDateTime = startTime,
                Type = "http://testType"
            };

            ConversationFacet conversation = new ConversationFacet
            {
                ConversationId = "conversation1",
                Turn = 100,
                Type = "http://conversationType"
            };

            conversation.AddToAnalyticsRecord(r);
            string actualJson = JsonConvert.SerializeObject(r, Formatting.Indented);
            var serializedRecord = JToken.Parse(actualJson);

            string targetJson = @"
            {
                '@context' : 'http://testContext',
                '@type' : 'http://testType', 
                '@id' : '11111',
                'receivedAtDateTime' : '2017-08-25T03:49:00-07:00',
                'botId' : '12345',
                'conversation' : {
                    '@type' : 'http://conversationType', 
                    'turn' : 100, 
                    'conversationId' : 'conversation1'
                }
            }";

            var desiredResult = JToken.Parse(targetJson);
            Assert.IsTrue(JToken.DeepEquals(desiredResult, serializedRecord), "Two JSON Objects are not equal");
        }

    }
}
