// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Bot.Builder.Adapters;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [CollectionDefinition("Dialogs.Adaptive.Recognizers")]
    public class EntityRecognizerTests
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        private static Lazy<EntityRecognizerSet> recognizers = new Lazy<EntityRecognizerSet>(() =>
        {
            return new EntityRecognizerSet()
            {
                new AgeEntityRecognizer(),
                new ConfirmationEntityRecognizer(),
                new CurrencyEntityRecognizer(),
                new DateTimeEntityRecognizer(),
                new DimensionEntityRecognizer(),
                new EmailEntityRecognizer(),
                new GuidEntityRecognizer(),
                new HashtagEntityRecognizer(),
                new IpEntityRecognizer(),
                new MentionEntityRecognizer(),
                new NumberEntityRecognizer(),
                new NumberRangeEntityRecognizer(),
                new OrdinalEntityRecognizer(),
                new PercentageEntityRecognizer(),
                new PhoneNumberEntityRecognizer(),
                new TemperatureEntityRecognizer(),
                new UrlEntityRecognizer(),
                new RegexEntityRecognizer() { Name = "color", Pattern = "(?i)(red|green|blue|purble|orange|violet|white|black)" },
                new RegexEntityRecognizer() { Name = "size", Pattern = "(?i)(small|medium|large)" },
            };
        });

        [Fact]
        public void TestAge()
        {
            var turnContext = GetTurnContext(nameof(TestAge), "This is a test of one, 2, three years old");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(6, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "age").ToList());
        }

        [Fact]
        public void TestConfirmation()
        {
            var turnContext = GetTurnContext(nameof(TestConfirmation), "yes, please");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(2, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "boolean").ToList());
        }

        [Fact]
        public void TestCurrency()
        {
            var turnContext = GetTurnContext(nameof(TestCurrency), "I would pay four dollars for that.");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(3, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "currency").ToList());
        }

        [Fact]
        public void TestDateTime()
        {
            var turnContext = GetTurnContext(nameof(TestDateTime), "Next thursday at 4pm.");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(4, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "datetimeV2.datetime").ToList());
            Assert.Single(results.Where(entity => entity.Type == "ordinal.relative").ToList());
            Assert.Single(results.Where(entity => entity.Type == "dimension").ToList());
        }

        [Fact]
        public void TestDimension()
        {
            var turnContext = GetTurnContext(nameof(TestDimension), "I think he's 5 foot ten");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(4, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "dimension").ToList());
        }

        [Fact]
        public void TestEmail()
        {
            var turnContext = GetTurnContext(nameof(TestEmail), "my email address is foo@att.uk.co");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(2, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "email").ToList());
        }

        [Fact]
        public void TestGuid()
        {
            var guid = Guid.Empty;
            var turnContext = GetTurnContext(nameof(TestGuid), $"my account number is {guid}...");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(7, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "guid").ToList());
        }

        [Fact]
        public void TestHashtag()
        {
            var turnContext = GetTurnContext(nameof(TestHashtag), $"I'm so cool #cool #groovy...");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(3, results.Count);
            Assert.Equal(2, results.Where(entity => entity.Type == "hashtag").Count());
        }

        [Fact]
        public void TestIp()
        {
            var turnContext = GetTurnContext(nameof(TestIp), $"My address is 1.2.3.4");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(6, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "ip").ToList());
        }

        [Fact]
        public void TestMention()
        {
            var turnContext = GetTurnContext(nameof(TestMention), $"Tell @joesmith I'm coming...");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(2, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "mention").ToList());
        }

        [Fact]
        public void TestNumber()
        {
            var turnContext = GetTurnContext(nameof(TestNumber), "This is a test of one, 2, three");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(4, results.Count);
            Assert.Equal(3, results.Where(entity => entity.Type == "number").Count());
        }

        [Fact]
        public void TestNumberRange()
        {
            var turnContext = GetTurnContext(nameof(TestNumberRange), "there are 3 to 5 of them");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(4, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "numberrange").ToList());
        }

        [Fact]
        public void TestOrdinal()
        {
            var turnContext = GetTurnContext(nameof(TestOrdinal), "First, second or third");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(4, results.Count);
            Assert.Equal(3, results.Where(entity => entity.Type == "ordinal").Count());
        }

        [Fact]
        public void TestPercentage()
        {
            var turnContext = GetTurnContext(nameof(TestPercentage), "The population hit 33.3%");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(3, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "percentage").ToList());
        }

        [Fact]
        public void TestPhoneNumber()
        {
            var turnContext = GetTurnContext(nameof(TestPhoneNumber), "Call 425-882-8080");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(5, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "phonenumber").ToList());
        }

        [Fact]
        public void TestTemperature()
        {
            var turnContext = GetTurnContext(nameof(TestTemperature), "set the oven to 350 degrees");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(3, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "temperature").ToList());
        }

        [Fact]
        public void TestUrl()
        {
            var turnContext = GetTurnContext(nameof(TestUrl), "go to http://about.me for more info");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(2, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "url").ToList());
        }

        [Fact]
        public void TestRegEx()
        {
            // I would like {order} 
            var turnContext = GetTurnContext(nameof(TestRegEx), "I would like a red or Blue cat");
            var results = recognizers.Value.RecognizeEntitiesAsync(turnContext).Result;

            Assert.Equal(3, results.Count);
            Assert.Equal(2, results.Where(entity => entity.Type == "color").Count());
            Assert.Equal("red", results[1].Properties["text"]);
            Assert.Equal("Blue", results[2].Properties["text"]);
        }

        private DialogContext GetTurnContext(string testName, string text, string locale = "en-us")
        {
            return new DialogContext(
                new DialogSet(),
                new TurnContext(
                    new TestAdapter(TestAdapter.CreateConversation(testName)),
                    new Schema.Activity(type: Schema.ActivityTypes.Message, text: text, locale: locale)),
                new DialogState());
        }
    }
}
