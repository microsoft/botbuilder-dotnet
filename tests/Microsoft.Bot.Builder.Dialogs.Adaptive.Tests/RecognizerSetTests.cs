// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Moq;
using Xunit;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Tests.ColorAndCodeUtils;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [CollectionDefinition("Dialogs.Adaptive.Recognizers")]
    public class RecognizerSetTests : IClassFixture<ResourceExplorerFixture>
    {
        private static readonly Lazy<RecognizerSet> Recognizers = new Lazy<RecognizerSet>(() =>
        {
            return new RecognizerSet()
            {
                Recognizers = new List<Recognizer>()
                {
                    new RegexRecognizer()
                    {
                        Id = "CodeRecognizer",
                        Intents = new List<IntentPattern>()
                        {
                            new IntentPattern("codeIntent", "(?<code>[a-z][0-9])"),
                        },
                        Entities = new EntityRecognizerSet()
                        {
                            new AgeEntityRecognizer(),
                            new NumberEntityRecognizer(),
                            new PercentageEntityRecognizer(),
                            new PhoneNumberEntityRecognizer(),
                            new TemperatureEntityRecognizer()
                        }
                    },
                    new RegexRecognizer()
                    {
                        Id = "ColorRecognizer",
                        Intents = new List<IntentPattern>()
                        {
                            new IntentPattern("colorIntent", "(?i)(color|colour)")
                        },
                        Entities = new EntityRecognizerSet()
                        {
                            new UrlEntityRecognizer(),
                            new RegexEntityRecognizer() { Name = "color", Pattern = "(?i)(red|green|blue|purple|orange|violet|white|black)" },
                            new RegexEntityRecognizer() { Name = "backgroundColor", Pattern = "(?i)(back|background)" },
                            new RegexEntityRecognizer() { Name = "foregroundColor", Pattern = "(?i)(foreground|front) {color}" }
                        }
                    }
                }
            };
        });

        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public RecognizerSetTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(RecognizerSetTests));
        }

        [Fact]
        public async Task RecognizerSetTests_Merge()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task RecognizerSetTests_None()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task RecognizerSetTests_Merge_LogsTelemetry_WhenLogPiiTrue()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizers = Recognizers.Value;
            recognizers.TelemetryClient = telemetryClient.Object;

            var dc = TestUtils.CreateContext(CodeIntentText);
            var activity = dc.Context.Activity;
            var result = await recognizers.RecognizeAsync(dc, activity, CancellationToken.None);
            ValidateCodeIntent(result);
            ValidateTelemetry(recognizers, telemetryClient, dc, activity, result, callCount: 1);

            dc = TestUtils.CreateContext(ColorIntentText);
            activity = dc.Context.Activity;
            result = await recognizers.RecognizeAsync(dc, activity, CancellationToken.None);
            ValidateColorIntent(result);
            ValidateTelemetry(recognizers, telemetryClient, dc, activity, result, callCount: 2);

            // Test custom activity
            dc = TestUtils.CreateContext(string.Empty);
            var customActivity = Activity.CreateMessageActivity();
            customActivity.Text = CodeIntentText;
            customActivity.Locale = Culture.English;
            result = await recognizers.RecognizeAsync(dc, (Activity)customActivity, CancellationToken.None);
            ValidateCodeIntent(result);
        }
    }
}
