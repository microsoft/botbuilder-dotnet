// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Moq;
using Xunit;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Tests.IntentValidations;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Tests.RecognizerTelemetryUtils;

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
            var recognizer = GetRecognizer();

            // test with DC
            var dc = TestUtils.CreateContext(CodeIntentText);
            var result = await recognizer.RecognizeAsync(dc, dc.Context.Activity, CancellationToken.None);
            ValidateCodeIntent(result);

            // verify seed text is not exposed
            dynamic entities = result.Entities;
            Assert.Null(entities.text);
            Assert.NotNull(entities.code);

            dc = TestUtils.CreateContext(ColorIntentText);
            result = await recognizer.RecognizeAsync(dc, dc.Context.Activity, CancellationToken.None);
            ValidateColorIntent(result);

            // test custom activity
            dc = TestUtils.CreateContext(string.Empty);
            var activity = Activity.CreateMessageActivity();
            activity.Text = "intent a1 b2";
            activity.Locale = Culture.English;
            result = await recognizer.RecognizeAsync(dc, (Activity)activity, CancellationToken.None);
            ValidateCodeIntent(result);

            activity.Text = "I would like color red and orange";
            result = await recognizer.RecognizeAsync(dc, (Activity)activity, CancellationToken.None);
            ValidateColorIntent(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task LogsTelemetry(bool logPersonalInformation)
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();

            var recognizer = GetRecognizer();
            recognizer.TelemetryClient = telemetryClient.Object;
            recognizer.LogPersonalInformation = logPersonalInformation;

            // Test with DC
            await RecognizeIntentAndValidateTelemetry(CodeIntentText, recognizer, telemetryClient, callCount: 1);
            await RecognizeIntentAndValidateTelemetry(ColorIntentText, recognizer, telemetryClient, callCount: 2);

            // Test custom activity
            await RecognizeIntentAndValidateTelemetry_WithCustomActivity(CodeIntentText, recognizer, telemetryClient, callCount: 3);
            await RecognizeIntentAndValidateTelemetry_WithCustomActivity(ColorIntentText, recognizer, telemetryClient, callCount: 4);
        }

        [Fact]
        public async Task LogPiiFalseByDefault()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = new RegexRecognizer { TelemetryClient = telemetryClient.Object };
            var dc = TestUtils.CreateContext("Salutations!");
            var (logPersonalInformation, _) = recognizer.LogPersonalInformation.TryGetValue(dc.State);

            Assert.False(logPersonalInformation);

            var result = await recognizer.RecognizeAsync(dc, dc.Context.Activity, CancellationToken.None);
            Assert.NotNull(result);
        }

        private static RegexRecognizer GetRecognizer() => new RegexRecognizer
        {
            Intents = new List<IntentPattern>
            {
                new IntentPattern("codeIntent", "(?<code>[a-z][0-9])"),
                new IntentPattern("colorIntent", "(?i)(color|colour)"),
            },
            Entities = new EntityRecognizerSet
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
                new RegexEntityRecognizer
                {
                    Name = "color",
                    Pattern = "(?i)(red|green|blue|purple|orange|violet|white|black)"
                },
                new RegexEntityRecognizer
                {
                    Name = "backgroundColor",
                    Pattern = "(?i)(back|background) {color}"
                },
                new RegexEntityRecognizer
                {
                    Name = "foregroundColor",
                    Pattern = "(?i)(foreground|front) {color}"
                },
            }
        };
    }
}
