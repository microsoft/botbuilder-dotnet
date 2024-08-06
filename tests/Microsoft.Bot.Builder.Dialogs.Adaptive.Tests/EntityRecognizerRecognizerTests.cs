// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                    new ChannelMentionEntityRecognizer(),
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
        public async Task TestAgeAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestAgeAsync), "This is a test of one, 2, three years old");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.age);
            Assert.NotNull(entities.number);
            Assert.NotNull(entities["$instance"].age);
            Assert.NotNull(entities["$instance"].number);
        }

        [Fact]
        public async Task TestConfirmationAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestConfirmationAsync), "yes, please");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.boolean);
            Assert.Equal("yes", (string)entities.boolean[0]);
            Assert.NotNull(entities["$instance"].boolean);
        }

        [Fact]
        public async Task TestCurrencyAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestCurrencyAsync), "I would pay four dollars for that.");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.currency);
            Assert.Equal("four dollars", (string)entities.currency[0]);
            Assert.NotNull(entities["$instance"].currency);
        }

        [Fact]
        public async Task TestDateTimeAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestDateTimeAsync), "Next thursday at 4pm.");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities["datetimeV2.datetime"]);
            Assert.NotNull(entities["ordinal.relative"]);
            Assert.NotNull(entities.dimension);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestDimensionAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestDimensionAsync), "I think he's 5 foot ten");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.number);
            Assert.NotNull(entities.dimension);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestEmailAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestEmailAsync), "my email address is foo@att.uk.co");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.email);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestGuidAsync()
        {
            var guid = Guid.Empty;
            var dialogContext = GetDialogContext(nameof(TestGuidAsync), $"my account number is {guid}...");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.guid);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestHashtagAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestHashtagAsync), $"I'm so cool #cool #groovy...");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.hashtag);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestIpAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestIpAsync), $"My address is 1.2.3.4");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.ip);
            Assert.NotNull(entities.number);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestMentionAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestMentionAsync), $"Tell @joesmith I'm coming...");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            dynamic instanceData = entities["$instance"];
            Assert.NotNull(entities.mention);
            Assert.Equal("@joesmith", (string)entities.mention[0]);
            Assert.Equal("@joesmith", (string)instanceData.mention[0].text);
            var startIndex = (int)instanceData.mention[0].startIndex;
            var endIndex = (int)instanceData.mention[0].endIndex;
            Assert.Equal("@joesmith", dialogContext.Context.Activity.Text.Substring(startIndex, endIndex - startIndex));
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestChannelMentionEntityRecognizerAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestMentionAsync), $"joelee bobsm...");
            dialogContext.Context.Activity.Entities = new List<Entity>();

            dynamic mention = new JObject();
            mention.type = "mention";
            mention.mentioned = new JObject();
            mention.mentioned.id = "15";
            mention.mentioned.name = "Joe Lee";
            mention.text = "joelee";
            dialogContext.Context.Activity.Entities.Add(((JObject)mention).ToObject<Entity>());

            mention = new JObject();
            mention.type = "mention";
            mention.mentioned = new JObject();
            mention.mentioned.id = "30";
            mention.mentioned.name = "Bob Smithson";
            mention.text = "bobsm";
            dialogContext.Context.Activity.Entities.Add(((JObject)mention).ToObject<Entity>());

            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            dynamic instanceData = entities["$instance"];

            // resolution [0]
            Assert.NotNull(entities.channelMention);
            Assert.Equal("15", (string)entities.channelMention[0].id);
            Assert.Equal("Joe Lee", (string)entities.channelMention[0].name);

            // instancedata[0]
            Assert.Equal("joelee", (string)instanceData.channelMention[0].text);
            var startIndex = (int)instanceData.channelMention[0].startIndex;
            var endIndex = (int)instanceData.channelMention[0].endIndex;
            Assert.Equal("joelee", dialogContext.Context.Activity.Text.Substring(startIndex, endIndex - startIndex));

            // resolution [1]
            Assert.Equal("30", (string)entities.channelMention[1].id);
            Assert.Equal("Bob Smithson", (string)entities.channelMention[1].name);

            // instanceData [1]
            Assert.Equal("bobsm", (string)instanceData.channelMention[1].text);
            startIndex = (int)instanceData.channelMention[1].startIndex;
            endIndex = (int)instanceData.channelMention[1].endIndex;
            Assert.Equal("bobsm", dialogContext.Context.Activity.Text.Substring(startIndex, endIndex - startIndex));
        }

        [Fact]
        public async Task TelemetryDoesNotLogByDefault()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = new ChannelMentionEntityRecognizer()
            {
                TelemetryClient = telemetryClient.Object
            };
            var dialogContext = GetDialogContext(nameof(TelemetryDoesNotLogByDefault), "gobble gobble");
            var (logPersonalInformation, _) = recognizer.LogPersonalInformation.TryGetValue(dialogContext.State);
            Assert.False(logPersonalInformation);

            var result = await recognizer.RecognizeAsync(dialogContext, dialogContext.Context.Activity, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Empty(result.Intents);
            Assert.Empty(result.Entities);
            Assert.Empty(telemetryClient.Invocations);
        }

        [Fact]
        public async Task TestNumberAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestNumberAsync), "This is a test of one, 2, three");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.number);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestNumberRangeAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestNumberRangeAsync), "there are 3 to 5 of them");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.numberrange);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestOrdinalAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestOrdinalAsync), "First, second or third");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.ordinal);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestPercentageAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestPercentageAsync), "The population hit 33.3%");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.percentage);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestPhoneNumberAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestPhoneNumberAsync), "Call 425-882-8080");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.phonenumber);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestTemperatureAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestTemperatureAsync), "set the oven to 350 degrees");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.temperature);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestUrlAsync()
        {
            var dialogContext = GetDialogContext(nameof(TestUrlAsync), "go to http://about.me for more info");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.url);
            Assert.Null(entities.boolean);
        }

        [Fact]
        public async Task TestRegExAsync()
        {
            // I would like {order} 
            var dialogContext = GetDialogContext(nameof(TestRegExAsync), "I would like a red or Blue cat");
            var results = await recognizers.Value.RecognizeAsync(dialogContext, dialogContext.Context.Activity);

            Assert.Single(results.Intents);
            Assert.Equal("None", results.Intents.Single().Key);

            dynamic entities = results.Entities;
            Assert.NotNull(entities.color);
            Assert.Null(entities.boolean);

            Assert.Equal(2, entities.color.Count);
            Assert.Equal("red", (string)entities.color[0]);
            Assert.Equal("Blue", (string)entities.color[1]);
        }

        [Fact]
        public async Task TestTelemetryDoesNotLogByDefault()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = new EntityRecognizer()
            {
                TelemetryClient = telemetryClient.Object
            };
            var dialogContext = GetDialogContext(nameof(TestTelemetryDoesNotLogByDefault), "gobble gobble");

            var (logPersonalInformation, _) = recognizer.LogPersonalInformation.TryGetValue(dialogContext.State);
            Assert.False(logPersonalInformation);

            var result = await recognizer.RecognizeAsync(dialogContext, dialogContext.Context.Activity, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Empty(result.Intents);
            Assert.Empty(result.Entities);
            Assert.Empty(telemetryClient.Invocations);
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
