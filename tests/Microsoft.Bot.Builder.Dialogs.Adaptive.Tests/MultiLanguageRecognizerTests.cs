// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Moq;
using Xunit;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Tests.RecognizerTelemetryUtils;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [CollectionDefinition("Dialogs.Adaptive.Recognizers")]
    public class MultiLanguageRecognizerTests : IClassFixture<ResourceExplorerFixture>
    {
        private static readonly Lazy<MultiLanguageRecognizer> Recognizer = new Lazy<MultiLanguageRecognizer>(() =>
        {
            return new MultiLanguageRecognizer()
            {
                Recognizers = new ConcurrentDictionary<string, Recognizer>(
                    new Dictionary<string, Recognizer>()
                    {
                        {
                            "en-us",
                            new RegexRecognizer()
                            {
                                Intents = new List<IntentPattern>()
                                {
                                    new IntentPattern("Greeting", "(?i)howdy"),
                                    new IntentPattern("Goodbye", "(?i)bye")
                                }
                            }
                        },
                        {
                            "en-gb",
                            new RegexRecognizer()
                            {
                                Intents = new List<IntentPattern>()
                                {
                                    new IntentPattern("Greting", "(?i)hiya"),
                                    new IntentPattern("Goodbye", "(?i)cheerio")
                                }
                            }
                        },
                        {
                            "en",
                            new RegexRecognizer()
                            {
                                Intents = new List<IntentPattern>()
                                {
                                    new IntentPattern("Greeting", "(?i)hello"),
                                    new IntentPattern("Goodbye", "(?i)goodbye")
                                }
                            }
                        }
                    })
            };
        });
        
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
        public async Task MultiLanguageRecognizerTest_Telemetry_LogsPii_WhenTrue()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = Recognizer.Value;
            recognizer.TelemetryClient = telemetryClient.Object;
            recognizer.LogPersonalInformation = true;

            await RecognizeIntentAndValidateTelemetry_WithCustomActivity(GreetingIntentTextEnUs, recognizer, telemetryClient, callCount: 1);
        }
        
        [Fact]
        public async Task MultiLanguageRecognizerTest_Telemetry_DoesntLogPii_WhenFalse()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = Recognizer.Value;
            recognizer.TelemetryClient = telemetryClient.Object;
            recognizer.LogPersonalInformation = false;

            await RecognizeIntentAndValidateTelemetry_WithCustomActivity(GreetingIntentTextEnUs, recognizer, telemetryClient, callCount: 1);
        }

        [Fact]
        public async Task MultiLanguageRecognizerTest_LogPii_IsFalseByDefault()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var recognizer = Recognizer.Value;
            recognizer.TelemetryClient = telemetryClient.Object;
            
            var dc = TestUtils.CreateContext(GreetingIntentTextEnUs);
            var activity = dc.Context.Activity;
            activity.Locale = "en-us";

            var (logPersonalInformation, _) = recognizer.LogPersonalInformation.TryGetObject(dc.State);
            Assert.Equal(false, logPersonalInformation);

            var result = await recognizer.RecognizeAsync(dc, activity, CancellationToken.None);
            ValidateGreetingIntent(result);
        }
    }
}
