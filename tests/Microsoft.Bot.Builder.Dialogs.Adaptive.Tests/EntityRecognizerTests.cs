// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task TestAgeAsync()
        {
            var turnContext = GetTurnContext(nameof(TestAgeAsync), "This is a test of one, 2, three years old");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(6, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "age").ToList());
        }

        [Fact]
        public async Task TestConfirmationAsync()
        {
            var turnContext = GetTurnContext(nameof(TestConfirmationAsync), "yes, please");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(2, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "boolean").ToList());
        }

        [Fact]
        public async Task TestCurrencyAsync()
        {
            var turnContext = GetTurnContext(nameof(TestCurrencyAsync), "I would pay four dollars for that.");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(3, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "currency").ToList());
        }

        [Fact]
        public async Task TestDateTimeAsync()
        {
            var turnContext = GetTurnContext(nameof(TestDateTimeAsync), "Next thursday at 4pm.");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(4, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "datetimeV2.datetime").ToList());
            Assert.Single(results.Where(entity => entity.Type == "ordinal.relative").ToList());
            Assert.Single(results.Where(entity => entity.Type == "dimension").ToList());
        }

        [Fact]
        public async Task TestDimensionAsync()
        {
            var turnContext = GetTurnContext(nameof(TestDimensionAsync), "I think he's 5 foot ten");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(4, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "dimension").ToList());
        }

        [Fact]
        public async Task TestEmailAsync()
        {
            var turnContext = GetTurnContext(nameof(TestEmailAsync), "my email address is foo@att.uk.co");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(2, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "email").ToList());
        }

        [Fact]
        public async Task TestGuidAsync()
        {
            var guid = Guid.Empty;
            var turnContext = GetTurnContext(nameof(TestGuidAsync), $"my account number is {guid}...");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(7, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "guid").ToList());
        }

        [Fact]
        public async Task TestHashtagAsync()
        {
            var turnContext = GetTurnContext(nameof(TestHashtagAsync), $"I'm so cool #cool #groovy...");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(3, results.Count);
            Assert.Equal(2, results.Where(entity => entity.Type == "hashtag").Count());
        }

        [Fact]
        public async Task TestIpAsync()
        {
            var turnContext = GetTurnContext(nameof(TestIpAsync), $"My address is 1.2.3.4");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(6, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "ip").ToList());
        }

        [Fact]
        public async Task TestMentionAsync()
        {
            var turnContext = GetTurnContext(nameof(TestMentionAsync), $"Tell @joesmith I'm coming...");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(2, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "mention").ToList());
        }

        [Fact]
        public async Task TestNumberAsync()
        {
            var turnContext = GetTurnContext(nameof(TestNumberAsync), "This is a test of one, 2, three");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(4, results.Count);
            Assert.Equal(3, results.Where(entity => entity.Type == "number").Count());
        }

        [Fact]
        public async Task TestNumberRangeAsync()
        {
            var turnContext = GetTurnContext(nameof(TestNumberRangeAsync), "there are 3 to 5 of them");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(4, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "numberrange").ToList());
        }

        [Fact]
        public async Task TestOrdinalAsync()
        {
            var turnContext = GetTurnContext(nameof(TestOrdinalAsync), "First, second or third");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(4, results.Count);
            Assert.Equal(3, results.Where(entity => entity.Type == "ordinal").Count());
        }

        [Fact]
        public async Task TestPercentageAsync()
        {
            var turnContext = GetTurnContext(nameof(TestPercentageAsync), "The population hit 33.3%");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(3, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "percentage").ToList());
        }

        [Fact]
        public async Task TestPhoneNumberAsync()
        {
            var turnContext = GetTurnContext(nameof(TestPhoneNumberAsync), "Call 425-882-8080");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(5, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "phonenumber").ToList());
        }

        [Fact]
        public async Task TestTemperatureAsync()
        {
            var turnContext = GetTurnContext(nameof(TestTemperatureAsync), "set the oven to 350 degrees");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(3, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "temperature").ToList());
        }

        [Fact]
        public async Task TestUrlAsync()
        {
            var turnContext = GetTurnContext(nameof(TestUrlAsync), "go to http://about.me for more info");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(2, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "url").ToList());
        }

        [Fact]
        public async Task TestRegExAsync()
        {
            // I would like {order} 
            var turnContext = GetTurnContext(nameof(TestRegExAsync), "I would like a red or Blue cat");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

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
