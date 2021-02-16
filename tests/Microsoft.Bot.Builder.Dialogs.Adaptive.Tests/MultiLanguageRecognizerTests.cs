// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Moq;
using Xunit;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Tests.IntentValidations;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Tests.RecognizerTelemetryUtils;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [CollectionDefinition("Dialogs.Adaptive.Recognizers")]
    public class MultiLanguageRecognizerTests : IClassFixture<ResourceExplorerFixture>
    {
        private readonly ResourceExplorerFixture _resourceExplorerFixture;

        public MultiLanguageRecognizerTests(ResourceExplorerFixture resourceExplorerFixture)
        {
            _resourceExplorerFixture = resourceExplorerFixture.Initialize(nameof(MultiLanguageRecognizerTests));
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_EnUsFallback()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_EnUsFallback_ActivityLocaleCasing()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_EnGbFallback()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_EnFallback()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_DefaultFallback()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_LanguagePolicy()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_LocaleCaseInsensitivity()
        {
            await TestUtils.RunTestScript(_resourceExplorerFixture.ResourceExplorer);
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

            await RecognizeIntentAndValidateTelemetry_WithCustomActivity(GreetingIntentTextEnUs, recognizer, telemetryClient,  1);
        }

        [Fact]
        public async Task LogPiiIsFalseByDefault()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = GetRecognizer();
            recognizer.TelemetryClient = telemetryClient.Object;

            var dc = TestUtils.CreateContext(GreetingIntentTextEnUs);
            var activity = dc.Context.Activity;
            activity.Locale = "en-us";

            var (logPersonalInformation, _) = recognizer.LogPersonalInformation.TryGetValue(dc.State);
            Assert.False(logPersonalInformation);

            var result = await recognizer.RecognizeAsync(dc, activity, CancellationToken.None);
            ValidateGreetingIntent(result);
        }

        private static MultiLanguageRecognizer GetRecognizer() => new MultiLanguageRecognizer
        {
            Recognizers = new Dictionary<string, Recognizer>
            {
                {
                    "en-us", new RegexRecognizer
                    {
                        Intents = new List<IntentPattern>
                        {
                            new IntentPattern("Greeting", "(?i)howdy"),
                            new IntentPattern("Goodbye", "(?i)bye")
                        }
                    }
                },
                {
                    "en-gb", new RegexRecognizer
                    {
                        Intents = new List<IntentPattern>
                        {
                            new IntentPattern("Greeting", "(?i)hiya"),
                            new IntentPattern("Goodbye", "(?i)cheerio")
                        }
                    }
                },
                {
                    "en", new RegexRecognizer
                    {
                        Intents = new List<IntentPattern>
                        {
                            new IntentPattern("Greeting", "(?i)hello"),
                            new IntentPattern("Goodbye", "(?i)goodbye")
                        }
                    }
                }
            }
        };
    }
}
