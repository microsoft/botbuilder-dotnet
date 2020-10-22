using System;
using System.Linq;
using Microsoft.Bot.Builder.Adapters;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [CollectionDefinition("Dialogs.Adaptive.Recognizers")]
    public class EntityRecognizerRecognizerTests
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        private static Lazy<RecognizerSet> recognizers = new Lazy<RecognizerSet>(() =>
        {
            return new RecognizerSet()
            {
                Recognizers = new System.Collections.Generic.List<Recognizer>()
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
                    new RegexEntityRecognizer() { Name = "color", Pattern = "(?i)(red|green|blue|purple|orange|violet|white|black)" },
                    new RegexEntityRecognizer() { Name = "size", Pattern = "(?i)(small|medium|large)" },
                }
            };
        });

        [Fact]
        public void TestAge()
        {
            var dialogContext = GetDialogContext(nameof(TestAge), "This is a test of one, 2, three years old");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.age);
            Assert.NotNull(entities.number);
            Assert.NotNull(entities["$instance"].age);
            Assert.NotNull(entities["$instance"].number);
        }

        [Fact]
        public void TestConfirmation()
        {
            var dialogContext = GetDialogContext(nameof(TestConfirmation), "yes, please");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.boolean);
            Assert.Equal("yes", (string)entities.boolean[0]);
            Assert.NotNull(entities["$instance"].boolean);
        }

        [Fact]
        public void TestCurrency()
        {
            var dialogContext = GetDialogContext(nameof(TestCurrency), "I would pay four dollars for that.");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.currency);
            Assert.Equal("four dollars", (string)entities.currency[0]);
            Assert.NotNull(entities["$instance"].currency);
        }

        [Fact]
        public void TestDateTime()
        {
            var dialogContext = GetDialogContext(nameof(TestDateTime), "Next thursday at 4pm.");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities["datetimeV2.datetime"]);
            Assert.NotNull(entities["ordinal.relative"]);
            Assert.NotNull(entities.dimension);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestDimension()
        {
            var dialogContext = GetDialogContext(nameof(TestDimension), "I think he's 5 foot ten");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.number);
            Assert.NotNull(entities.dimension);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestEmail()
        {
            var dialogContext = GetDialogContext(nameof(TestEmail), "my email address is foo@att.uk.co");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.email);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestGuid()
        {
            var guid = Guid.Empty;
            var dialogContext = GetDialogContext(nameof(TestGuid), $"my account number is {guid}...");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.guid);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestHashtag()
        {
            var dialogContext = GetDialogContext(nameof(TestHashtag), $"I'm so cool #cool #groovy...");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.hashtag);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestIp()
        {
            var dialogContext = GetDialogContext(nameof(TestIp), $"My address is 1.2.3.4");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.ip);
            Assert.NotNull(entities.number);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestMention()
        {
            var dialogContext = GetDialogContext(nameof(TestMention), $"Tell @joesmith I'm coming...");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.mention);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestNumber()
        {
            var dialogContext = GetDialogContext(nameof(TestNumber), "This is a test of one, 2, three");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.number);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestNumberRange()
        {
            var dialogContext = GetDialogContext(nameof(TestNumberRange), "there are 3 to 5 of them");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.numberrange);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestOrdinal()
        {
            var dialogContext = GetDialogContext(nameof(TestOrdinal), "First, second or third");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.ordinal);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestPercentage()
        {
            var dialogContext = GetDialogContext(nameof(TestPercentage), "The population hit 33.3%");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.percentage);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestPhoneNumber()
        {
            var dialogContext = GetDialogContext(nameof(TestPhoneNumber), "Call 425-882-8080");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.phonenumber);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestTemperature()
        {
            var dialogContext = GetDialogContext(nameof(TestTemperature), "set the oven to 350 degrees");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.temperature);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestUrl()
        {
            var dialogContext = GetDialogContext(nameof(TestUrl), "go to http://about.me for more info");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.url);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public void TestRegEx()
        {
            // I would like {order} 
            var dialogContext = GetDialogContext(nameof(TestRegEx), "I would like a red or Blue cat");
            var results = recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity).Result;

            Assert.Equal(1, results.Intents.Count);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.color);
            Assert.Null(entities.boolean);

            Assert.Equal(2, entities.color.Count);
            Assert.Equal("red", (string)entities.color[0]);
            Assert.Equal("Blue", (string)entities.color[1]);
        }

        private DialogContext GetDialogContext(string testName, string text, string locale = "en-us")
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
