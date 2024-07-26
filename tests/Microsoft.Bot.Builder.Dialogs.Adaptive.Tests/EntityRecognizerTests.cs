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

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestConfirmation()
After:
        public void TestConfirmationAsync()
*/
        public async Task TestConfirmationAsync()
        {
            var turnContext = GetTurnContext(nameof(TestConfirmationAsync), "yes, please");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(2, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "boolean").ToList());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestCurrency()
After:
        public void TestCurrencyAsync()
*/
        public async Task TestCurrencyAsync()
        {
            var turnContext = GetTurnContext(nameof(TestCurrencyAsync), "I would pay four dollars for that.");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(3, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "currency").ToList());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestDateTime()
After:
        public void TestDateTimeAsync()
*/
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

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestDimension()
After:
        public void TestDimensionAsync()
*/
        public async Task TestDimensionAsync()
        {
            var turnContext = GetTurnContext(nameof(TestDimensionAsync), "I think he's 5 foot ten");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(4, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "dimension").ToList());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestEmail()
After:
        public void TestEmailAsync()
*/
        public async Task TestEmailAsync()
        {
            var turnContext = GetTurnContext(nameof(TestEmailAsync), "my email address is foo@att.uk.co");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(2, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "email").ToList());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestGuid()
After:
        public void TestGuidAsync()
*/
        public async Task TestGuidAsync()
        {
            var guid = Guid.Empty;
            var turnContext = GetTurnContext(nameof(TestGuidAsync), $"my account number is {guid}...");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(7, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "guid").ToList());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestHashtag()
After:
        public void TestHashtagAsync()
*/
        public async Task TestHashtagAsync()
        {
            var turnContext = GetTurnContext(nameof(TestHashtagAsync), $"I'm so cool #cool #groovy...");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(3, results.Count);
            Assert.Equal(2, results.Where(entity => entity.Type == "hashtag").Count());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestIp()
After:
        public void TestIpAsync()
*/
        public async Task TestIpAsync()
        {
            var turnContext = GetTurnContext(nameof(TestIpAsync), $"My address is 1.2.3.4");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(6, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "ip").ToList());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestMention()
After:
        public void TestMentionAsync()
*/
        public async Task TestMentionAsync()
        {
            var turnContext = GetTurnContext(nameof(TestMentionAsync), $"Tell @joesmith I'm coming...");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(2, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "mention").ToList());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestNumber()
After:
        public void TestNumberAsync()
*/
        public async Task TestNumberAsync()
        {
            var turnContext = GetTurnContext(nameof(TestNumberAsync), "This is a test of one, 2, three");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(4, results.Count);
            Assert.Equal(3, results.Where(entity => entity.Type == "number").Count());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestNumberRange()
After:
        public void TestNumberRangeAsync()
*/
        public async Task TestNumberRangeAsync()
        {
            var turnContext = GetTurnContext(nameof(TestNumberRangeAsync), "there are 3 to 5 of them");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(4, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "numberrange").ToList());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestOrdinal()
After:
        public void TestOrdinalAsync()
*/
        public async Task TestOrdinalAsync()
        {
            var turnContext = GetTurnContext(nameof(TestOrdinalAsync), "First, second or third");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(4, results.Count);
            Assert.Equal(3, results.Where(entity => entity.Type == "ordinal").Count());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestPercentage()
After:
        public void TestPercentageAsync()
*/
        public async Task TestPercentageAsync()
        {
            var turnContext = GetTurnContext(nameof(TestPercentageAsync), "The population hit 33.3%");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(3, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "percentage").ToList());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestPhoneNumber()
After:
        public void TestPhoneNumberAsync()
*/
        public async Task TestPhoneNumberAsync()
        {
            var turnContext = GetTurnContext(nameof(TestPhoneNumberAsync), "Call 425-882-8080");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(5, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "phonenumber").ToList());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestTemperature()
After:
        public void TestTemperatureAsync()
*/
        public async Task TestTemperatureAsync()
        {
            var turnContext = GetTurnContext(nameof(TestTemperatureAsync), "set the oven to 350 degrees");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(3, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "temperature").ToList());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestUrl()
After:
        public void TestUrlAsync()
*/
        public async Task TestUrlAsync()
        {
            var turnContext = GetTurnContext(nameof(TestUrlAsync), "go to http://about.me for more info");
            var results = await recognizers.Value.RecognizeEntitiesAsync(turnContext);

            Assert.Equal(2, results.Count);
            Assert.Single(results.Where(entity => entity.Type == "url").ToList());
        }

        [Fact]

/* Unmerged change from project 'Microsoft.Bot.Builder.Dialogs.Adaptive.Tests (net8.0)'
Before:
        public void TestRegEx()
After:
        public void TestRegExAsync()
*/
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
