// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Moq;
using Xunit;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Tests.RecognizerTelemetryUtils;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [CollectionDefinition("Dialogs.Adaptive.Recognizers")]
    public class CrossTrainedRecognizerSetTests : IClassFixture<ResourceExplorerFixture>
    {
        private static readonly Lazy<CrossTrainedRecognizerSet> Recognizers = new Lazy<CrossTrainedRecognizerSet>(() =>
        {
            return new CrossTrainedRecognizerSet()
            {
                Recognizers = new List<Recognizer>()
                {
                    new RegexRecognizer()
                    {
                        Id = "x",
                        Intents = new List<IntentPattern>()
                        {
                            new IntentPattern("x", CrossTrainText),
                            new IntentPattern("x", "x")
                        }
                    },
                    new RegexRecognizer()
                    {
                        Id = "CodeRecognizer",
                        Intents = new List<IntentPattern>()
                        {
                            new IntentPattern("DeferToRecognizer_x", CrossTrainText),
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
                            new IntentPattern("y", CrossTrainText),
                            new IntentPattern("colorIntent", "(?i)(color|colour)"),
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

        public CrossTrainedRecognizerSetTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(CrossTrainedRecognizerSetTests));
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_DoubleDefer()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_CircleDefer()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_DoubleIntent()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_NoneWithIntent()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_AllNone()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_NoneIntentWithEntities()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }
        
        [Fact]
        public async Task CrossTrainedRecognizerSetTests_Empty()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task CrossTrainedRecognizerSetTests_Telemetry_LogsPii_WhenTrue()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizers = Recognizers.Value;
            recognizers.TelemetryClient = telemetryClient.Object;
            recognizers.LogPersonalInformation = true;

            await RecognizeIntentAndValidateTelemetry(CrossTrainText, recognizers, telemetryClient, callCount: 1);
        }
    }
}
