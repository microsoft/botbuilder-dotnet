// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [CollectionDefinition("Dialogs.Adaptive.Recognizers")]
    public class RegexRecognizerTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public RegexRecognizerTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(RegexRecognizerTests));
        }

        [Fact]
        public async Task RegexRecognizerTests_Entities()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task RegexRecognizerTests_Intents()
        {
            var recognizer = new RegexRecognizer()
            {
                Intents = new List<IntentPattern>()
                {
                     new IntentPattern("codeIntent", "(?<code>[a-z][0-9])"),
                     new IntentPattern("colorIntent", "(?i)(color|colour)"),
                },
                Entities = new EntityRecognizerSet()
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
                    new RegexEntityRecognizer() { Name = "backgroundColor", Pattern = "(?i)(back|background) {color}" },
                    new RegexEntityRecognizer() { Name = "foregroundColor", Pattern = "(?i)(foreground|front) {color}" },
                }
            };

            // test with DC
            var dc = CreateContext("intent a1 b2");
            var result = await recognizer.RecognizeAsync(dc, dc.Context.Activity, CancellationToken.None);
            ValidateCodeIntent(result);

            // verify seed text is not exposed
            dynamic entities = result.Entities;
            Assert.Null(entities.text);
            Assert.NotNull(entities.code);

            dc = CreateContext("I would like color red and orange");
            result = await recognizer.RecognizeAsync(dc, dc.Context.Activity, CancellationToken.None);
            ValidateColorIntent(result);

            dc = CreateContext(string.Empty);

            // test custom activity
            var activity = Activity.CreateMessageActivity();
            activity.Text = "intent a1 b2";
            activity.Locale = Culture.English;
            result = await recognizer.RecognizeAsync(dc, (Activity)activity, CancellationToken.None);
            ValidateCodeIntent(result);

            activity.Text = "I would like color red and orange";
            result = await recognizer.RecognizeAsync(dc, (Activity)activity, CancellationToken.None);
            ValidateColorIntent(result);
        }

        private static void ValidateColorIntent(RecognizerResult result)
        {
            Assert.Single(result.Intents);
            Assert.Equal("colorIntent", result.Intents.Select(i => i.Key).First());

            // entity assertions from capture group
            dynamic entities = result.Entities;
            Assert.NotNull(entities.color);
            Assert.Null(entities.code);
            Assert.Equal(2, entities.color.Count);
            Assert.Equal("red", (string)entities.color[0]);
            Assert.Equal("orange", (string)entities.color[1]);
        }

        private static void ValidateCodeIntent(RecognizerResult result)
        {
            // intent assertions
            Assert.Single(result.Intents);
            Assert.Equal("codeIntent", result.Intents.Select(i => i.Key).First());

            // entity assertions from capture group
            dynamic entities = result.Entities;
            Assert.NotNull(entities.code);
            Assert.Null(entities.color);
            Assert.Equal(2, entities.code.Count);
            Assert.Equal("a1", (string)entities.code[0]);
            Assert.Equal("b2", (string)entities.code[1]);
        }

        private static DialogContext CreateContext(string text)
        {
            var activity = Activity.CreateMessageActivity();
            activity.Text = text;
            return new DialogContext(new DialogSet(), new TurnContext(new TestAdapter(), (Activity)activity), new DialogState());
        }
    }
}
